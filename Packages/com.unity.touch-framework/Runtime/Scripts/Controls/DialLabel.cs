using UnityEngine;

namespace Unity.TouchFramework
{

    // Converts dial labels from default float to desired format
    public interface ILabelConverter
    {
        string ConvertTickLabels(float value); // The white tick labels on dial
        string ConvertSelectedValLabel(float value, bool isInt); // The current (selected value) blue label
    }

    class DefaultConverter : ILabelConverter
    {
        public string ConvertTickLabels(float value)
        {
            return value.ToString("F1");
        }
        public string ConvertSelectedValLabel(float value, bool isInt)
        {
            if (isInt)
            {
                return Mathf.Round(value).ToString();
            }
            else
            {
                return value.ToString("F2");
            }
        }
    }
}
