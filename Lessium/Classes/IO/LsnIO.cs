namespace Lessium.Classes.IO
{
    public enum IOResult
    {
        Null, Error, Successful, Cancelled, Timeout
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
