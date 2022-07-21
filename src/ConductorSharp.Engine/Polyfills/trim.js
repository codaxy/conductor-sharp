function (str, chars, mode) {

    if (chars == null)
        chars = ' ';

    var trimStart = true;
    var trimEnd = true;

    if (mode)
    {
        trimStart = mode === "start";
        trimEnd = mode === "end";
    }

    var map = {};

    if (typeof chars == 'string')
        chars = [chars];

    for (var i = 0; i < chars.length; ++i) {
        map[chars[i]] = true;
    }

    var front = 0;
    var back = str.length - 1;

    for (var i = 0; i <= Math.floor(str.length / 2); ++i) {
        if (trimStart && map[str[front]])
            ++front;
        if (trimEnd && map[str[back]])
            --back;
    }

    if (back < front)
        return "";
    else
        return str.substring(front, back + 1);
}