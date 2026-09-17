using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.Utilities.ResultResponse
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Message { get; }
        public object? Data { get; }
        public List<string>? Errors { get; }

        protected Result(bool isSuccess,object? data,string? message,List<string>? errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            Errors = errors;
        }

        public static Result Success(string? message = null,object? data = null)
        {
            return new Result(
                true,
                data,
                message,
                null);
        }

        public static Result Failure(string message,List<string>? errors = null)
        {
            return new Result(
                false,
                null,
                message,
                errors);
        }
    }

    public class Result<T> : Result
    {
        private Result(bool isSuccess,T? data,string? message,List<string>? errors): base(isSuccess, data, message, errors)
        {
        }

        public static Result<T> Success(T data,string? message = null)
        {
            return new Result<T>(
                true,
                data,
                message,
                null);
        }

        public static Result<T> Failure(string message,List<string>? errors = null)
        {
            return new Result<T>(
                false,
                default,
                message,
                errors);
        }
    }


}
