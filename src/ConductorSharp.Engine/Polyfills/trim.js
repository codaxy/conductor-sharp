function (str, chars) {

    if (chars == null)
        chars = ' ';

    let map = {};

    if (typeof chars == 'string')
        chars = [chars];

    for (let i = 0; i < chars.length; ++i) {
        map[chars[i]] = true;
    }

    let front = 0;
    let back = str.length - 1;

    for (let i = 0; i <= Math.floor(str.length / 2); ++i) {
        if (map[str[front]])
            ++front;
        if (map[str[back]])
            --back;
    }

    if (back < front)
        return "";
    else
        return str.substring(front, back + 1);
}