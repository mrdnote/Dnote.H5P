// ReSharper disable once NativeTypePrototypeExtending
String.prototype.setUrlParam = function (param, value)
{
    if (value === null) value = "";

    let url = this;

    var regex = new RegExp("([?]" + param + "=)[^\&#]*", "i");
    var regex2 = new RegExp("([&]" + param + "=)[^\&#]*", "i");
    if (regex.test(url))
    {
        if (value === "")
        {
            return url.replace(regex, "?");
        }
        return url.replace(regex, "$1" + encodeURIComponent(value));
    } else if (regex2.test(url))
    {
        if (value === "")
        {
            return url.replace(regex2, "");
        }
        return url.replace(regex2, "$1" + encodeURIComponent(value));
    } else
    {
        if (value !== "")
        {
            var separator = "?";
            if (url.indexOf("?") > -1)
                separator = "&";
            var anchorIdx = url.indexOf('#');
            var anchor = null;
            if (anchorIdx > -1)
            {
                anchor = url.substring(anchorIdx);
                url = url.substring(0, anchorIdx);
            }
            var result = url + separator + param + "=" + encodeURIComponent(value);
            if (anchor !== null)
                result += anchor;
            return result;
        }
        return url;
    }
};
