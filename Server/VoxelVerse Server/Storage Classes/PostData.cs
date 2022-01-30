using System;
using System.Collections.Generic;

public class PostData
{
    public string Title;
    public int Views;

    public List<PostItemData> Items = new List<PostItemData>();

    public class PostItemData
    {
        public string Content;
        public Guid Publisher;
        public string Date;
    }

    public void Serialize(ref Packet packet, bool local)
    {
        packet.Write(Title);
        packet.Write(Views);

        packet.Write(Items.Count);
        for (int i = 0; i < Items.Count; i++)
        {
            AccountData ac = Program.Accounts[Items[i].Publisher];

            packet.Write(Items[i].Content);
            packet.Write(Items[i].Date);

            if (!local) { packet.Write(ac.Username); packet.Write(ac.Icon); }
            else { packet.Write(Items[i].Publisher); }
        }
    }

    public static PostData Deserialize(ref Packet packet)
    {
        PostData postData = new PostData()
        {
            Title = packet.ReadString(),
            Views = packet.ReadInt()
        };

        int postItems = packet.ReadInt();
        for (int i = 0; i < postItems; i++)
        {
            postData.Items.Add(new PostItemData()
            {
                Content = packet.ReadString(),
                Publisher = packet.ReadGuid(),
                Date = packet.ReadString()
            });
        }

        return postData;
    }
}