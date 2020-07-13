var common = function () {

};
common.timeToHumanReadable = function (secs) {
    var result = "";
    var numdays = Math.floor((secs % 31536000) / 86400);
    var numhours = Math.floor(((secs % 31536000) % 86400) / 3600);
    var numminutes = Math.floor((((secs % 31536000) % 86400) % 3600) / 60);
    var numseconds = Math.floor((((secs % 31536000) % 86400) % 3600) % 60);
    if (numseconds > 0) {
        result = numseconds + " сек.";
    }
    if (numminutes > 0) {
        result = numminutes + " мин. " + result;
    }
    if (numhours > 0) {
        result = numhours + " ч. " + result;
    }
    if (numdays > 0) {
        result = numdays + " д. " + result;
    }
    if (!result) {
        result = "0 ч. 0 мин. 0 сек.";
    }
    return result;
};
common.timeToReadable = function (secs) {
    var result = "";
    var numdays = Math.floor((secs % 31536000) / 86400);
    var numhours = Math.floor(((secs % 31536000) % 86400) / 3600);
    var numminutes = Math.floor((((secs % 31536000) % 86400) % 3600) / 60);
    var numseconds = Math.floor((((secs % 31536000) % 86400) % 3600) % 60);
    if (numseconds > 0) {
        result = numseconds;
    } else {
        result = "00";
    }
    if (numminutes > 0) {
        result = numminutes + ":" + result;
    }
    if (numhours > 0) {
        result = numhours + " ч. " + result;
    }
    if (!result) {
        result = "00:00:00";
    }
    return result;
};
common.distanceReadable = function (d) {
    if (!d)
        return 0;
    return (Math.round(d / 100) / 10);
};
common.parseISO8601 = function (dateStringInRange) {
    var isoExp = /^\s*(\d{4})-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)\s*$/,
        date, month,
        parts = isoExp.exec(dateStringInRange);
    if (!parts) {
        isoExp = /^\s*(\d{4})-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d).(\d*)\s*$/;
        parts = isoExp.exec(dateStringInRange);
    }
    if (parts) {
        month = +parts[2];
        date = new Date(parts[1], month - 1, parts[3], parts[4], parts[5], parts[6]);
        if (month != date.getMonth() + 1) {
            date.setTime(NaN);
        }
    }
    return date;
};
common.stringToTimePart = function (s) {
    return Ext.Date.format(common.parseISO8601(s), 'H:i:s');
};
common.stringToDatePart = function (s) {
    return Ext.Date.format(common.parseISO8601(s), 'd-m-Y');
};
common.stringTimesDiff = function (s, e) {
    var st = common.parseISO8601(s),
        et = common.parseISO8601(e);
    if (!st || !et) {
        return '';
    }
    var diff = new Date(et.getTime() - st.getTime());
    var hoursDifference = Math.floor(diff / 1000 / 60 / 60);
    diff -= hoursDifference * 1000 * 60 * 60;

    var minutesDifference = Math.floor(diff / 1000 / 60);
    diff -= minutesDifference * 1000 * 60;

    var secondsDifference = Math.floor(diff / 1000);

    return Ext.Date.format(new Date(2000, 1, 1, hoursDifference, minutesDifference, secondsDifference), 'H:i:s');
};

var triggerRender = function (item) {
    item.inputEl.dom.onmousedown = name;
};

function name(t, t1, t2) {
    //var ctx = document.getElementById(this.id);
    //var ex = Ext.get(this.id);
    App[Ext.get(this.id).findParent('.x-field').id].onTriggerClick();
}

function bytesToSize(bytes) {
    if ((bytes >> 30) & 0x3FF)
        bytes = (bytes >>> 30) + '.' + (bytes & (3 * 0x3FF)) + ' GB';
    else if ((bytes >> 20) & 0x3FF)
        bytes = (bytes >>> 20) + '.' + (bytes & (2 * 0x3FF)) + ' MB';
    else if ((bytes >> 10) & 0x3FF)
        bytes = (bytes >>> 10) + '.' + (bytes & (0x3FF)) + ' KB';
    else if ((bytes >> 1) & 0x3FF)
        bytes = (bytes >>> 1) + ' Bytes';
    else
        bytes = bytes + ' Byte';
    return bytes == ' Byte' ? '' : bytes;
}