using System.Collections.Generic;

public class ArticleData
{
    public string Title;
    public string Publisher;
    public string Date;

    public string Content;
    public byte[] Images;

    public List<CommentData> Comments = new List<CommentData>();

    public void Serialize(ref Packet packet)
    {
        packet.Write(Title);
        packet.Write(Publisher);
        packet.Write(Date);

        packet.Write(Content);
        // Images here

        packet.Write(Comments.Count);
        for (int i = 0; i < Comments.Count; i++)
        {
            packet.Write(Comments[i].Date);
            packet.Write(Comments[i].Publisher);

            packet.Write(Comments[i].Content);
        }
    }
}

public class CommentData
{
    public string Date;
    public string Publisher;

    public string Content;
}