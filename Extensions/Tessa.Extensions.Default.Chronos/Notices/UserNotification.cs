using System;
using System.Collections.Generic;
using System.Linq;

namespace Tessa.Extensions.Default.Chronos.Notices
{
    public class UserNotification
    {
        #region Constructors

        public UserNotification(Guid userID, string normalizedWebAddress)
        {
            this.UserID = userID;
            this.WebAddress = normalizedWebAddress;

            this.OutdatedTasks = new List<TaskNotificationInfo>();
            this.OutdatedTasksInProgress = new List<TaskNotificationInfo>();
            this.Tasks = new List<TaskNotificationInfo>();
            this.TasksInProgress = new List<TaskNotificationInfo>();
            this.AutoApprovedTasks = new List<AutoAprovedTaskNotificationInfo>();
        }

        #endregion

        #region Properties

        public Guid UserID { get; private set; }
        public string WebAddress { get; private set; }

        public List<TaskNotificationInfo> OutdatedTasks { get; set; }
        public List<TaskNotificationInfo> OutdatedTasksInProgress { get; set; }
        public List<TaskNotificationInfo> Tasks{ get; set; }
        public List<TaskNotificationInfo> TasksInProgress { get; set; }
        public List<AutoAprovedTaskNotificationInfo> AutoApprovedTasks { get; set; }

        public Dictionary<string, object> GetInfo()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [nameof(OutdatedTasks)] = OutdatedTasks.Select(x => (object)x.GetStorage()).ToList(),
                [nameof(OutdatedTasksInProgress)] = OutdatedTasksInProgress.Select(x => (object)x.GetStorage()).ToList(),
                [nameof(Tasks)] = Tasks.Select(x => (object)x.GetStorage()).ToList(),
                [nameof(TasksInProgress)] = TasksInProgress.Select(x => (object)x.GetStorage()).ToList(),
                [nameof(AutoApprovedTasks)] = AutoApprovedTasks.Select(x => (object)x.GetStorage()).ToList(),
                ["HasWeb"] = !string.IsNullOrWhiteSpace(WebAddress),
            };
        }

        #endregion
    }
}
