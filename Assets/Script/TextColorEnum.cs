using UnityEngine;

public enum TextColorEnum
{
    None,
    Red,
    Blue,
    Green,
}

public static class TextColor
{
    public static Color GetColorFromEnum(TextColorEnum colorEnum)
    {
        switch (colorEnum)
        {
            case TextColorEnum.Red:
                return Color.red;

            case TextColorEnum.Blue:
                return Color.blue;

            case TextColorEnum.Green:
                return Color.green;

            default: return Color.white;
        }
    }
}