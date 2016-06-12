using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Fancy.ResourceLinker
{
    /// <summary>
    /// Class with extension methods for the "UrlHelper" to easily create links to controller methods.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Helper method to create links to controller via linq expressions.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="methodExpression">The method expression.</param>
        /// <returns>The constructed link.</returns>
        public static string LinkTo<TController>(this IUrlHelper urlHelper, Expression<Action<TController>> methodExpression) where TController : Controller
        {
            if (urlHelper == null) throw new ArgumentNullException(nameof(urlHelper));
            if (methodExpression == null) throw new ArgumentNullException(nameof(methodExpression));

            var methodCallExpression = methodExpression.Body as MethodCallExpression;
            return LinkTo(urlHelper, methodCallExpression);
        }

        /// <summary>
        /// Builds a link to a controller method.
        /// </summary>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="methodCallExpression">The method call expression.</param>
        /// <returns>The constructed link.</returns>
        private static string LinkTo(IUrlHelper urlHelper, MethodCallExpression methodCallExpression)
        {
            // Read the route parameters and convert them into a dictionary containing the value to each param
            Dictionary<string, object> routeValues = methodCallExpression.Method.GetParameters()
                .Select(p => GetParameterValue(methodCallExpression, p))
                .ToDictionary(p => p.Item1, p => p.Item2);

            // Try to read the route attribute from the method
            RouteAttribute routeAttribute = methodCallExpression.Method.GetCustomAttribute<RouteAttribute>();

            if(routeAttribute == null)
            {
                // Try to read the route attribute from the type
                routeAttribute = methodCallExpression.Method.DeclaringType.GetTypeInfo().GetCustomAttribute<RouteAttribute>();
            }

            // If no route was found use the default route
            string routeName = routeAttribute != null ? routeAttribute.Name : "DefaultApi";

            if (string.IsNullOrEmpty(routeName))
            {
                throw new InvalidOperationException("The name property of the Route attribute needs to be set to use the attribute route for linking");
            }

            // Use the url helper to create a link
            return urlHelper.Link(routeName, routeValues);
        }

        /// <summary>
        /// Extracts the parameter value to from one parameter.
        /// </summary>
        /// <param name="methodCallExpression">The method call expression.</param>
        /// <param name="parameterInfo">The parameter information.</param>
        /// <returns>The parameter values.</returns>
        private static Tuple<string, object> GetParameterValue(MethodCallExpression methodCallExpression, ParameterInfo parameterInfo)
        {
            Expression arg = methodCallExpression.Arguments[parameterInfo.Position];
            LambdaExpression lambda = Expression.Lambda(arg);

            object value = lambda.Compile().DynamicInvoke();

            return new Tuple<string, object>(parameterInfo.Name, value);
        }
    }
}