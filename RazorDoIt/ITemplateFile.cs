namespace RazorDoIt
{
    public interface ITemplateFile
    {
        bool Exists(string path);
        string GetTemplate(string path);
    }
}
