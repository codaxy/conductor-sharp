function (str, width, char, mode) {

    if (width <= str.length)
        return str;

    var padding = char.repeat(width - str.length);

    return (mode == "left" ? padding : "") + str + (mode == "right" ? padding : "");
}