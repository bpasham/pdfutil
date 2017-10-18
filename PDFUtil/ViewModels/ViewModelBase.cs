using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
//using SachaBarber.WaitScreen;


namespace PDFUtil.ViewModels
{
    /// <summary>
    /// Provides common functionality for ViewModel classes
    /// </summary>
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region Constructor       
       
        protected ViewModelBase()
        {
        }

        #endregion // Constructor
                    
        #region OverRidables
        public bool _isBusy;
        string _busyMessage;
        
        #region DisplayName
        string _displayName;
        
        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value,
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                this.OnPropertyChanged(p => p.DisplayName);
            }
        }

        #endregion // DisplayName

        public virtual bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                this.OnPropertyChanged(p => p.IsBusy);
            }
        }

        public virtual string BusyMessage
        {
            get { return _busyMessage; }
            protected set
            {
                _busyMessage = value;
                this.OnPropertyChanged(p => p.BusyMessage);
            }
        }

        #endregion
        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }


        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        public void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            this.OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

#if DEBUG
        /// <summary>
        /// Useful for ensuring that ViewModel objects are properly garbage collected.
        /// </summary>
        ~ViewModelBase()
        {
            //string msg = string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this.DisplayName, this.GetHashCode());
            //System.Diagnostics.Debug.WriteLine(msg);
        }
#endif

        #endregion // IDisposable Members        
        
        #region Threadables
        public delegate void TaskCompletedEventHandler(object sender, TaskCompletedEventArgs e);

        /// <summary>
        /// Raised when the async task is completed
        /// </summary>
        public event TaskCompletedEventHandler TaskCompleted;

        /// <summary>
        /// Raised Task Completed, when the thread is done after the UI returns to normal.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        public void OnTaskCompleted(bool failed, object state, string message)
        {
            TaskCompletedEventHandler handler = this.TaskCompleted;
            if (handler != null)
            {
                var e = new TaskCompletedEventArgs()
                {                    
                    Failed = failed,
                    State = state,
                    Message=message
                };
                handler(this,e);
            }
        }

        #endregion //Threadables
    }

    public static class ViewModelBaseEx
    {
        public static void OnPropertyChanged<T, TProperty>(this T observableBase, Expression<Func<T, TProperty>> expression) where T : ViewModelBase
        {
            observableBase.OnPropertyChanged(observableBase.GetPropertyName(expression));
        }

        public static string GetPropertyName<T, TProperty>(this T owner, Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = expression.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;

                    if (memberExpression == null)
                        throw new NotImplementedException();
                }
                else
                    throw new NotImplementedException();
            }

            var propertyName = memberExpression.Member.Name;
            return propertyName;
        }

    }

    public class TaskCompletedEventArgs
    {
        public object State { get; set; }        
        public bool Failed { get; set; }
        public string Message { get; set; }
    }
}
