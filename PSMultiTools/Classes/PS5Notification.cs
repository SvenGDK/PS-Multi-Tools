using System.Collections.Generic;

namespace PSMultiTools.Classes
{
    public class Parameters
    {
        public string? icon { get; set; }

        public string? actionUrl { get; set; }
    }

    public class Icon
    {
        public Parameters? parameters { get; set; }

        public string? @type { get; set; }
    }

    public class Message
    {
        public string? body { get; set; }
    }

    public class ViewData
    {
        public Icon? icon { get; set; }

        public Message? message { get; set; }

        public List<Action>? actions { get; set; }

        public SubMessage? subMessage { get; set; }
    }

    public class PreviewDisabled
    {
        public ViewData? viewData { get; set; }
    }

    public class PlatformViews
    {
        public PreviewDisabled? previewDisabled { get; set; }
    }

    public class Action
    {
        public string? actionName { get; set; }

        public string? actionType { get; set; }

        public bool defaultFocus { get; set; }

        public Parameters? parameters { get; set; }
    }

    public class SubMessage
    {
        public string? body { get; set; }
    }

    public class PS5Notification
    {
        public string? bundleName { get; set; }

        public string? channelType { get; set; }

        public bool isAnonymous { get; set; }

        public bool isImmediate { get; set; }

        public PlatformViews? platformViews { get; set; }

        public int priority { get; set; }

        public string? toastOverwriteType { get; set; }

        public string? useCaseId { get; set; }

        public ViewData? viewData { get; set; }

        public string? viewTemplateType { get; set; }

    }
}