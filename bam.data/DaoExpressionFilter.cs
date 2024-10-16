﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bam.Logging;
using Bam;
using System.Reflection;

namespace Bam.Data
{
    public class DaoExpressionFilter: ExpressionVisitor, ILoggable
    {
        ILogger _logger;
        readonly DaoExpressionVisitorEventSource _eventEmitter;
        readonly StringBuilder _traceLog;
        readonly QueryFilter _filter;
        int _counter = 0;
        static List<ExpressionType> _comparisonTypes;

        public DaoExpressionFilter(ILogger logger = null)
        {
            _traceLog = new StringBuilder();
            _filter = new QueryFilter();
            _logger = logger ?? Log.Default;
            _eventEmitter = new DaoExpressionVisitorEventSource();
            _eventEmitter.Subscribe(_logger);
            _comparisonTypes = new List<ExpressionType>
            {
                ExpressionType.Equal,
                ExpressionType.NotEqual,
                ExpressionType.GreaterThanOrEqual,
                ExpressionType.GreaterThan,
                ExpressionType.LessThan,
                ExpressionType.LessThanOrEqual
            };
        }

        public VerbosityLevel LogVerbosity { get; set; }
        public ILogger[] Subscribers => _eventEmitter.Subscribers;

        public void Subscribe(ILogger logger)
        {
            _eventEmitter.Subscribe(logger);
        }
        public virtual void Subscribe(VerbosityLevel levelToSubscribe, Action<ILoggable, LoggableEventArgs> handler)
        {
            Type emittingType = _eventEmitter.GetType();
            EventInfo[] eventInfos = emittingType.GetEvents();
            eventInfos.Each(eventInfo =>
            {
                if (eventInfo.HasCustomAttributeOfType(out VerbosityAttribute verbosityAttribute))
                {
                    if (verbosityAttribute.Value == levelToSubscribe)
                    {
                        eventInfo.AddEventHandler(this, (EventHandler)((s, a) =>
                        {
                            handler(this, LoggableEventArgs.ForLoggable(this, verbosityAttribute));
                        }));
                    }
                }
            });
        }

        public bool IsSubscribed(ILogger logger)
        {
            return _eventEmitter.IsSubscribed(logger);
        }
        
        public QueryFilter Where<T>(Expression<Func<T, bool>> expression)
        {
            Visit(expression);
            return _filter;
        }

        protected internal string TraceLog => _traceLog.ToString();

        protected override Expression VisitBinary(BinaryExpression b)
        {
            FireVisiting("VisitBinary");
            ThrowIfNotComparison(b);
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            Expression conversion = this.Visit(b.Conversion);
            if (left != b.Left || right != b.Right || conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            }
            else
            {
                MemberExpression memberLeft = b.Left as MemberExpression;

                _filter.AddRange(GetFilterForExpression(new QueryFilter(GetColumnName(memberLeft.Member)), b.NodeType, GetValue(b.Right)));
            }
            FireVisited("VisitBinary");
            return b;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            FireVisiting("VisitMethodCall");
            Expression exp = base.VisitMethodCall(expression);
            FireVisited("VisitMethodCall");
            return exp;
        }

        protected void FireVisiting(string methodName)
        {
            string filter = _filter.Parse();
            _eventEmitter.FireVisiting(filter, methodName);
            _traceLog.AppendFormat("{0}:Visiting::{1}\r\n", ++_counter, methodName);
            _traceLog.AppendLine(filter);
        }

        protected void FireVisited(string methodName)
        {
            string filter = _filter.Parse();
            _eventEmitter.FireVisited(filter, methodName);
            _traceLog.AppendFormat("{0}:Visited::{1}\r\n", ++_counter, methodName);
            _traceLog.AppendLine(filter);
        }

        private static string GetColumnName(MemberInfo info)
        {
            if (info.HasCustomAttributeOfType(out ColumnAttribute attr))
            {
                return attr.Name;
            }
            return info.Name;
        }

        private static QueryFilter GetFilterForExpression(QueryFilter column, ExpressionType expressionType, object v)
        {
            QueryValue value = Query.Value(v);
            switch (expressionType)
            {
                case ExpressionType.NotEqual:
                    return column != value;
                case ExpressionType.GreaterThan:
                    return column > value;
                case ExpressionType.GreaterThanOrEqual:
                    return column >= value;
                case ExpressionType.LessThan:
                    return column < value;
                case ExpressionType.LessThanOrEqual:
                    return column <= value;
                case ExpressionType.Equal:
                default:
                    return column == value;
            }
        }
        private static object GetValue(Expression expression)
        {            
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    MemberExpression member = expression as MemberExpression;
                    UnaryExpression objectMember = Expression.Convert(member, typeof(object));

                    Expression<Func<object>> getterLambda = Expression.Lambda<Func<object>>(objectMember);

                    Func<object> getter = getterLambda.Compile();

                    return getter();
                case ExpressionType.Constant:
                    return ((ConstantExpression)expression).Value;
            }
            return null;            
        }
        
        private void ThrowIfNotComparison(Expression expression)
        {
            if (!_comparisonTypes.Contains(expression.NodeType))
            {
                throw new ExpressionTypeNotSupportedException(expression.NodeType);
            }
        }

        internal class DaoExpressionVisitorEventSource : Loggable
        {
            public DaoExpressionVisitorEventSource() : base() { }
            public string Filter { get; set; }
            public string CurrentMethod { get; set; }

            [Verbosity(VerbosityLevel.Information, SenderMessageFormat = "Visiting::{CurrentMethod}: \r\n{Filter}")]
            public event EventHandler Visiting;

            [Verbosity(VerbosityLevel.Information, SenderMessageFormat = "Visited::{CurrentMethod}: \r\n{Filter}")]
            public event EventHandler Visited;

            public void FireVisiting(string filter, string currentMethod)
            {
                this.Filter = filter;
                this.CurrentMethod = currentMethod;
                FireEvent(Visiting, EventArgs.Empty);
            }

            public void FireVisited(string filter, string currentMethod)
            {
                this.Filter = filter;
                this.CurrentMethod = currentMethod;
                FireEvent(Visited, EventArgs.Empty);
            }

            public new void FireEvent(EventHandler eventHandler, EventArgs e)
            {
                base.FireEvent(eventHandler, e);
            }
        }
    }
}
