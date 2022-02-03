using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    static class TextFieldExtensions
    {
        public static void SelectRangeDelayed(this TextField textField, int cursorIndex, int selectionIndex)
        {
            textField.schedule.Execute(() =>
            {
                textField.Q("unity-text-input").Focus();
                textField.SelectRange(cursorIndex, selectionIndex);
            });
        }
    }
}