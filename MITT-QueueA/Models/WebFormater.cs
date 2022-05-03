using System;
using System.Linq;
using System.Web;

namespace MITT_QueueA.Models
{
    public static class WebFormater
    {
        public static string GetDateString(DateTime date)
        {
            TimeSpan diff = DateTime.Now - date;
            if (Math.Floor((diff.Days / 30.417) / 12) > 0)
            {
                return Math.Floor((diff.Days / 30.417) / 12).ToString() + $" Year{(Math.Floor((diff.Days / 30.417) / 12) > 1 ? "s" : "")} ago";
            }
            else if (Math.Floor(diff.Days / 30.417) > 0)
            {
                return Math.Floor(diff.Days / 30.417).ToString() + $" Month{(Math.Floor(diff.Days / 30.417) > 1 ? "s" : "")} ago";
            }
            else if (diff.Days > 0)
            {
                return diff.Days.ToString() + $" Day{(diff.Days > 1 ? "s" : "")} ago";
            }
            else if (diff.Hours > 0)
            {
                return diff.Hours.ToString() + $" Hour{(diff.Days > 1 ? "s" : "")} ago";
            }
            else if (diff.Minutes > 0)
            {
                return diff.Minutes.ToString() + $" Minute{(diff.Minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                return diff.Seconds.ToString() + $" Second{(diff.Seconds > 1 ? "s" : "")} ago";
            }
        }

        public static IHtmlString GetUsernameHTMLString(ApplicationUser user)
        {
            return new HtmlString($"{user.Email} {((user.IdentityRoles.FirstOrDefault(r => r.Name == "Gold") != null)? "<span class=\"gold-badge\">&#10026;</span>" : "")} • {user.Reputation}");
        }
    }
}