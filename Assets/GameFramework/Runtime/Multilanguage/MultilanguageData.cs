

namespace GameFramework
{
    public class MultilanguageData
    {
        public string ID;//中文文本的HashCode
        public string Chinese = "";
        public string English = "";

        public MultilanguageData(string id, string text)
        {
            this.ID = id;
            Chinese = text;
        }
    }
}
