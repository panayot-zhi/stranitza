using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Serilog;

namespace stranitza.Utility
{
    /// <summary>
    /// https://www.briandunning.com/error-codes/?source=MySQL
    /// </summary>
    // 
    public enum KnownErrors
    {
        /// <summary>
        /// Error 1062 - SQLSTATE: 23000 (ER_DUP_ENTRY) Duplicate entry '%s' for key %d
        /// </summary>
        ER_DUP_ENTRY = 1062
    }

    public sealed class StranitzaDbErrorHandler
    {
        // Singleton pattern implementation
        private static readonly StranitzaDbErrorHandler instance = new StranitzaDbErrorHandler();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static StranitzaDbErrorHandler()
        {
        }

        private StranitzaDbErrorHandler()
        {
        }

        public static StranitzaDbErrorHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public void HandleError(ModelStateDictionary modelState, Exception ex)
        {
            // mark exception HResult and track it in log
            var errorMessage = $"Грешка ({ex.HResult}): ";

            if (ex is StranitzaException stranitzaEx)
            {
                errorMessage += stranitzaEx.Message;
                var key = ResolveModelKey(modelState, stranitzaEx.Message);
                modelState.AddModelError(key: key, errorMessage: errorMessage);
            }

            else if (ex is DbUpdateConcurrencyException dbConcurrentException)
            {
                errorMessage += "Вероятно междувременно са били запаметени конфликтни промени по тези данни, моля, презаредете страницата и опитайте отново.";
                modelState.AddModelError(key: string.Empty, errorMessage: errorMessage);
            }

            else if (ex is DbUpdateException)
            {
                var dbEx = FindException<MySqlException>(ex);
                if (dbEx == null)
                {                    
                    errorMessage += "Възникнала е грешка при запис на данните. Моля, проверете въведените данни и опитайте отново.";
                    modelState.AddModelError(key: string.Empty, errorMessage: errorMessage);
                }
                else
                {
                    errorMessage += ResolveErrorMessage(dbEx);
                    var key = ResolveModelKey(modelState, dbEx.Message);
                    modelState.AddModelError(key: key, errorMessage: errorMessage);
                }
            }
            else
            {
                // for further classification
                errorMessage += "Непозната системна грешка. Моля, свържете се със системния администратор с номера на грешката.";
                modelState.AddModelError(key: string.Empty, errorMessage: errorMessage);
            }            

            // always log error message
            errorMessage += "\r\n{Ex}";
            Log.Logger.Error(errorMessage, ex);
            return;

        }

        public static int? GetErrorNumber(Exception ex)
        {
            var dbEx = FindException<MySqlException>(ex);
            if (dbEx != null)
            {
                return dbEx.Number;
            }

            //Log.Logger.Warning("Could not resolve MySqlException number from exception: {Message}\r\n{Ex}", ex.Message, ex);

            return null;
        }

        public static KnownErrors? GetKnownError(Exception ex)
        {
            var dbEx = FindException<MySqlException>(ex);
            if (dbEx != null)
            {
                return GetKnownError(dbEx.Number);
            }

            return null;
        }

        public static KnownErrors? GetKnownError(int errorNumber)
        {
            if (Enum.IsDefined(typeof(KnownErrors), errorNumber))
            {
                return (KnownErrors) errorNumber;
            }

            return null;
        }

        public static string ResolveModelKey(ModelStateDictionary modelState, string errorMessage)
        {
            foreach (var modelStateKey in modelState.Keys)
            {
                if (errorMessage.Contains(modelStateKey, StringComparison.InvariantCulture))
                {
                    // key in message, value?
                    if (errorMessage.Contains($"'{modelState[modelStateKey].RawValue}'"))
                    {
                        return modelStateKey;
                    }
                }                
            }

            // not resolved
            return string.Empty;
        }

        public static string ResolveErrorMessage(MySqlException mySqlEx)
        {           
            switch (mySqlEx.Number)
            {                
                default:                    
                    return $"Грешка при операция с база данни. Моля, свържете се със системен администратор със следната информация: No. {mySqlEx.Number}, SqlState: {mySqlEx.SqlState}, Message: {mySqlEx.Message}";
            }
        }

        public static T FindException<T>(Exception exception) where T : Exception
        {
            while (exception != null && !(exception is T))
            {
                exception = exception.InnerException;
            }

            return exception as T;
        }


    }
}
