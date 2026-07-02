namespace DingTalkLib
{
    public class DingTalkAtSetting
    {
        public bool IsAtAll
        {
            get; set;
        } = false;

        /// <summary>
        /// 需求在文本内容中增加 "@手机号"才可以触发
        /// </summary>
        public string[]? AtMoblies
        {
            get; set;
        }

        public string[]? AtUsers
        {
            get; set;
        }
    }
}
