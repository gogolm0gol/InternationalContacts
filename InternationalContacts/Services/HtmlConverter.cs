using System.Xml.Xsl;

namespace InternationalContacts.Services;

public class HtmlConverter
{
    public void Convert(string xmlPath, string xslPath, string htmlPath)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(xslPath);
        xslt.Transform(xmlPath, htmlPath);
    }
}
