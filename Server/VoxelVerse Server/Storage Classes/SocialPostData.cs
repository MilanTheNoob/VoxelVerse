using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SocialPostData
{
    public Guid Publisher;
    public string Content;

    public bool UseImage;
    public byte[] AttachedImage;

    public bool UseDownload;
    public string DownloadName;

    public int LikeCount;
    public int CommentCount;
    public int ReshareCount;
}