[System.Serializable]
public struct intVector3
{
    public const float kEpsilon = 0.00001F;
    public const float kEpsilonNormalSqrt = 1e-15F;

    public int x;
    public int y;
    public int z;

    public intVector3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
    public intVector3(int x, int y) { this.x = x; this.y = y; z = 0; }

    static readonly intVector3 zeroVector = new intVector3(0, 0, 0);
    static readonly intVector3 oneVector = new intVector3(1, 1, 1);
    static readonly intVector3 upVector = new intVector3(0, 1, 0);
    static readonly intVector3 downVector = new intVector3(0, -1, 0);
    static readonly intVector3 leftVector = new intVector3(-1, 0, 0);
    static readonly intVector3 rightVector = new intVector3(1, 0, 0);
    static readonly intVector3 forwardVector = new intVector3(0, 0, 1);
    static readonly intVector3 backVector = new intVector3(0, 0, -1);

    public static intVector3 zero { get { return zeroVector; } }
    public static intVector3 one { get { return oneVector; } }
    public static intVector3 forward { get { return forwardVector; } }
    public static intVector3 back { get { return backVector; } }
    public static intVector3 up { get { return upVector; } }
    public static intVector3 down { get { return downVector; } }
    public static intVector3 left { get { return leftVector; } }
    public static intVector3 right { get { return rightVector; } }

    public static intVector3 operator +(intVector3 a, intVector3 b) { return new intVector3(a.x + b.x, a.y + b.y, a.z + b.z); }
    public static intVector3 operator -(intVector3 a, intVector3 b) { return new intVector3(a.x - b.x, a.y - b.y, a.z - b.z); }
    public static intVector3 operator -(intVector3 a) { return new intVector3(-a.x, -a.y, -a.z); }

    public static intVector3 operator *(intVector3 a, int d) { return new intVector3(a.x * d, a.y * d, a.z * d); }
    public static intVector3 operator *(int d, intVector3 a) { return new intVector3(a.x * d, a.y * d, a.z * d); }
    public static intVector3 operator /(intVector3 a, int d) { return new intVector3(a.x / d, a.y / d, a.z / d); }

    public static bool operator ==(intVector3 lhs, intVector3 rhs)
    {
        float diff_x = lhs.x - rhs.x;
        float diff_y = lhs.y - rhs.y;
        float diff_z = lhs.z - rhs.z;
        float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
        return sqrmag < kEpsilon * kEpsilon;
    }

    public static bool operator !=(intVector3 lhs, intVector3 rhs)
    {
        return !(lhs == rhs);
    }
}