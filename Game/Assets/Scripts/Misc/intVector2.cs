[System.Serializable]
public struct intVector2
{
    public const float kEpsilon = 0.00001F;
    public const float kEpsilonNormalSqrt = 1e-15F;

    public int x;
    public int y;

    public intVector2(int x, int y, int z) { this.x = x; this.y = y; }
    public intVector2(int x, int y) { this.x = x; this.y = y; }

    static readonly intVector2 zeroVector = new intVector2(0, 0, 0);
    static readonly intVector2 oneVector = new intVector2(1, 1, 1);
    static readonly intVector2 upVector = new intVector2(0, 1, 0);
    static readonly intVector2 downVector = new intVector2(0, -1, 0);
    static readonly intVector2 leftVector = new intVector2(-1, 0, 0);
    static readonly intVector2 rightVector = new intVector2(1, 0, 0);
    static readonly intVector2 forwardVector = new intVector2(0, 0, 1);
    static readonly intVector2 backVector = new intVector2(0, 0, -1);

    public static intVector2 zero { get { return zeroVector; } }
    public static intVector2 one { get { return oneVector; } }
    public static intVector2 forward { get { return forwardVector; } }
    public static intVector2 back { get { return backVector; } }
    public static intVector2 up { get { return upVector; } }
    public static intVector2 down { get { return downVector; } }
    public static intVector2 left { get { return leftVector; } }
    public static intVector2 right { get { return rightVector; } }

    public static intVector2 operator +(intVector2 a, intVector2 b) { return new intVector2(a.x + b.x, a.y + b.y); }
    public static intVector2 operator -(intVector2 a, intVector2 b) { return new intVector2(a.x - b.x, a.y - b.y); }
    public static intVector2 operator -(intVector2 a) { return new intVector2(-a.x, -a.y); }

    public static intVector2 operator *(intVector2 a, int d) { return new intVector2(a.x * d, a.y * d); }
    public static intVector2 operator *(int d, intVector2 a) { return new intVector2(a.x * d, a.y * d); }
    public static intVector2 operator /(intVector2 a, int d) { return new intVector2(a.x / d, a.y / d); }

    public static bool operator ==(intVector2 lhs, intVector2 rhs)
    {
        float diff_x = lhs.x - rhs.x;
        float diff_y = lhs.y - rhs.y;
        return (diff_x * diff_x + diff_y * diff_y) < kEpsilon * kEpsilon;
    }

    public static bool operator !=(intVector2 lhs, intVector2 rhs)
    {
        return !(lhs == rhs);
    }
}