namespace MHbinder.Core.binds   
{
    public class BinderItem
    {
        public string Trigger { get; set; }
        public string Replace { get; set; }

        public BinderItem(string trigger, string replace)
        {
            Trigger = trigger;
            Replace = replace;
        }
    }
}