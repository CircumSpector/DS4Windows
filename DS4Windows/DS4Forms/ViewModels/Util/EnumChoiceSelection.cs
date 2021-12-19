namespace DS4WinWPF.DS4Forms.ViewModels.Util
{
    public class EnumChoiceSelection<T>
    {
        public EnumChoiceSelection(string name, T currentValue)
        {
            DisplayName = name;
            ChoiceValue = currentValue;
        }

        public string DisplayName { get; }

        public T ChoiceValue { get; set; }
    }
}