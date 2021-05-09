namespace Lessium.Classes.IO
{
    public enum IOResult
    {
        Null, Error, successful, Cancelled, Timeout
    }

    public enum IOType
    {
        Unknown, Read, Write
    }

    public enum ProgressType
    {
        Tab, Section, Page, Content
    }
}
