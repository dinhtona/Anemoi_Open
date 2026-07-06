using System;
using System.Collections.Generic;

namespace Anemoi.Contract.Notification.Constants;

public static class NotificationConstants
{
    public static class Categories
    {
        public const string System = "System";
        public const string Workspace = "Workspace";
        public const string Task = "Task";
        public const string Environment = "Environment";
        public const string Leave = "Leave";
        public const string Overtime = "Overtime";
        public const string Payroll = "Payroll";
        public const string Recruitment = "Recruitment";
        public const string Workflow = "Workflow";

        public static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            System,
            Workspace,
            Task,
            Environment,
            Leave,
            Overtime,
            Payroll,
            Recruitment,
            Workflow
        };
    }

    public static class SignalRMethods
    {
        public const string ReceiveNotification = "ReceiveNotification";
        public const string UserOnline = "UserOnline";
        public const string UserOffline = "UserOffline";
        public const string ReceiveDataChange = "ReceiveDataChange";
    }

    public static class DataChangeActions
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
    }

    public static class DataSensitivity
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
    }

    public static class TargetTypes
    {
        public const string User = "User";
        public const string Role = "Role";
        public const string Group = "Group";
    }

    public static class Types
    {
        public const string Business = "Business";
        public const string System = "System";
        public const string Invalidation = "Invalidation";
    }

    public static class Severities
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Error = "Error";
    }

    public static class ActionTypes
    {
        public const string Navigate = "Navigate";
        public const string Command = "Command";
        public const string ExternalLink = "ExternalLink";
    }
}
