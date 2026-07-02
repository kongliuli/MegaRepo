namespace DingTalkLib
{
    public class DingTalkTest
    {
        public void t()
        {
            MessageContent markdown = new()
            {
                title="testmarkdown",
                text=""
            };

            MarkDownDingTalkClient md = new(
                markdown,
                "https://oapi.dingtalk.com/robot/send",
                "cf48aae08bb3cf2bf88a79b349c38ade78348d8571d70b21b56113df2fb74897",
                "SECac5f99d07087195fecc94ac270bbf5d76b8da57a5e4499c047474b851e71fcc2",
                null);

            var repo = md.IGetPostUri()
                                  .SendMessageAsync()
                                  .Response
                                  .Result
                                  .Content;
        }

    }
}
