using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
        /// Helper method to create links to controller via linq expressions for async controller methods.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="urlHelper">The URL helper.</param>
        /// <param name="methodExpression">The method expression.</param>
        /// <returns>The constructed link.</returns>
        public static string LinkTo<TController>(this IUrlHelper urlHelper, Expression<Func<TController, Task>> methodExpression) where TController : Controller
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

            string controller = methodCallExpression.Method.DeclaringType.Name.Replace("Controller", "");
            string action = methodCallExpression.Method.Name;

            string actionUrl = urlHelper.Action(action, controller, routeValues);
            string baseUrl;

            if(urlHelper.ActionContext.HttpContext.Request.Headers.ContainsKey("ResourceProxy"))
            {
                baseUrl = urlHelper.ActionContext.HttpContext.Request.Headers["ResourceProxy"];
            }
            else
            {
                baseUrl = urlHelper.ActionContext.HttpContext.Request.Scheme + "://" + urlHelper.ActionContext.HttpContext.Request.Host.Value;
            }

            return CombineUri(baseUrl, actionUrl);
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

        /// <summary>
        /// Combines a URI from two parts.
        /// </summary>
        /// <param name="uri1">The uri1.</param>
        /// <param name="uri2">The uri2.</param>
        /// <returns>The combined uri.</returns>
        public static string CombineUri(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return string.Format("{0}/{1}", uri1, uri2);
        }
    }
}