// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq;

namespace Microsoft.Linq.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Maintains a list of mappings between properties and their compiled expressions.
    /// </summary>
    public class TranslationMap : Dictionary<MemberInfo, CompiledExpression>
    {
        internal static readonly TranslationMap DefaultMap = new TranslationMap();

        public CompiledExpression<T, TResult> Get<T, TResult>(MethodBase method)
        {
            Argument.EnsureNotNull("method", method);

            var propertyInfo = method.DeclaringType.GetRuntimeProperty(method.Name.Replace("get_", String.Empty));
            return this[propertyInfo] as CompiledExpression<T, TResult>;
        }

        public void Add<T, TResult>(Expression<Func<T, TResult>> property, CompiledExpression<T, TResult> compiledExpression)
        {
            Argument.EnsureNotNull("property", property);
            Argument.EnsureNotNull("compiledExpression", compiledExpression);

            //Add(((MemberExpression)property.Body).Member, compiledExpression);

            var memberExpression = (MemberExpression) property.Body;
            var member = memberExpression.Member;

            if (!member.DeclaringType.IsAbstract)
            {
                Add(member, compiledExpression);
                return;
            }

            
            //The member access is abstract
            //write a new expression with list of "if( $e is Type) then return ($e as Type).Property" :

            var expressions = new Dictionary<Expression, Expression>();
            var returnTarget = Expression.Label(property.Body.Type);

            var concreteTypes = member.DeclaringType.Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(member.DeclaringType));
            foreach (var concreteType in concreteTypes)
            {
                var typeAsExpression = Expression.TypeAs(memberExpression.Expression, concreteType);
                var newMemberAccess = Expression.MakeMemberAccess(typeAsExpression, member);
                var returnExpression =  Expression.Return(returnTarget, newMemberAccess);
                var resultExpression = Expression.TypeIs(memberExpression.Expression, concreteType);

                expressions.Add(resultExpression, returnExpression);
            }

            var defaultReturnExpression = Expression.Default(property.Body.Type);
            var result = BuildIfTypeIsThenTypeAsForExpressionsList(0, expressions, Expression.Return(returnTarget, defaultReturnExpression));

            var block = Expression.Block(result, Expression.Label(returnTarget, defaultReturnExpression));

            var lambdaResult = Expression.Lambda<Func<T, TResult>>(block, property.Parameters);

            var compiledLambdaExpression = new CompiledExpression<T, TResult>(lambdaResult);

            var expressionBaseConcreteType = property.Type.GetProperties()[0].ReflectedType.GenericTypeArguments[0];
            var expressionConcreteMember =
                expressionBaseConcreteType.GetRuntimeProperties().SingleOrDefault(p => p.Name == member.Name);

            Add(expressionConcreteMember, compiledLambdaExpression);
        }

        private Expression BuildIfTypeIsThenTypeAsForExpressionsList(int index, Dictionary<Expression, Expression> expressions, GotoExpression defaultReturnExpression)
        {
            return Expression.IfThenElse(
                expressions.Keys.ElementAt(index),
                expressions.Values.ElementAt(index),
                index < expressions.Count - 1
                    ? BuildIfTypeIsThenTypeAsForExpressionsList(index + 1, expressions, defaultReturnExpression)
                    : defaultReturnExpression);
        }

        public CompiledExpression<T, TResult> Add<T, TResult>(Expression<Func<T, TResult>> property, Expression<Func<T, TResult>> expression)
        {
            Argument.EnsureNotNull("property", property);
            Argument.EnsureNotNull("expression", expression);

            var compiledExpression = new CompiledExpression<T, TResult>(expression);
            Add(property, compiledExpression);
            return compiledExpression;
        }
    }
}