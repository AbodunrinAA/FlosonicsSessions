using Microsoft.AspNetCore.Mvc;

namespace FlosonicsSession.Helpers;

public static class ResponseExtensions
{
    public static IActionResult WithETag(this IActionResult result, string value, string key="ETag")
    {
        result = new ActionResultWithHeader(result, key, value);
        return result;
    }

    private class ActionResultWithHeader : IActionResult
    {
        private readonly IActionResult _result;
        private readonly string _key;
        private readonly string _value;

        public ActionResultWithHeader(IActionResult result, string key, string value)
        {
            _result = result;
            _key = key;
            _value = value;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add(_key, _value);
            await _result.ExecuteResultAsync(context);
        }
    }
}
