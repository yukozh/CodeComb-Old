var page = 0;
var id = null;
var lock = false;

function Load()
{
    if (lock) return;
    lock = true;
    if ($("#iconLoading").length > 0)
        $("#iconLoading").show();

    LoadRatings();
    LoadTopics();
    LoadReplies();
    LoadContests();
    LoadStatuses();
    LoadSolutionTags();
}

function LoadRatings()
{
    if ($("#lstRatings").length > 0) {
        $.getJSON("/Rating/GetRatings", {
            page: page,
            rnd: Math.random()
        }, function (ratings) {
            if (ratings.length == 0) { $("#iconLoading").hide(); lock = true; return;}//尾页锁定
            for (var i in ratings)
            {
                $("#lstRatings").append('<a class="rank-item" href="/User/' + ratings[i].UserID + '">'
                                                 + '    <div class="rank-face float-left"><img src="' + ratings[i].Gravatar + '"></div>'
                                                 + '    <div class="rank-cont float-right"><div class="rank-name"><h2>' + ratings[i].Nickname + '</h2></div><div class="rank-value">Credit: ' + ratings[i].Credit + '</div></div>'
                                                 + '    <div class="clear"></div>'
                                                 + '    <div class="rank-rank">#' + ratings[i].Rank + '</div>'
                                                 + '</a>');
            }
            lock = false;
            page++;
            $("#iconLoading").hide();
        });
    }
}
function LoadTopics()
{
    if ($("#lstTopics").length > 0) {
        $.getJSON("/Topic/GetTopics", {
            page: page,
            id: id,
            rnd: Math.random()
        }, function (topics) {
            if (page == 0 && topics.length == 0) {
                $("#lstTopics").removeClass('shadow');
                $("#lstTopics").append('<p>该板块无主题</p>');
            }
            if (topics.length == 0) { $("#iconLoading").hide(); lock = true; return; }//尾页锁定
           
            for (var i in topics) {
                $("#lstTopics").append('<div class="post-item forum-post-item ' + (topics[i].Top ? 'forum-post-item-highlight' : '') + '">'
                                                 + '    <div class="post-title"><h3><a href="/Topic/' + topics[i].ID + '">' + topics[i].Title + '</a></h3></div>'
                                                 + '    <div class="post-info"><a href="/User/' + topics[i].UserID + '"' + topics[i].Nickname + '</a> ' + topics[i].Time + ' 发表于 <a href="/Forum/' + topics[i].ForumID + '">' + topics[i].ForumTitle + '</a> ' + (topics[i].HasReply ? '最新回复：<a href="/User/' + topics[i].LastReplyUserID + '">' + topics[i].LastReplyNickname + '</a> @' + topics[i].LastReplyTime : '') + '</div>'
                                                 + '    <div class="forum-post-face"><img src="' + topics[i].Gravatar + '"></div>'
                                                 + '    <div class="forum-post-replies">' + topics[i].RepliesCount + '</div>'
                                                 + '</div>');
            }
            lock = false;
            page++;
            $("#iconLoading").hide();
        });
    }
}
function LoadReplies() {
    if ($("#lstReplies").length > 0) {
        $.post("/Reply/GetReplies/" + id, {
            page: page,
            rnd: Math.random()
        }, function (replies) {
            if (replies.length < 20) { $("#iconLoading").hide(); lock = true; return; }//尾页锁定
            $("#lstReplies").append(replies);
            PostReplyBinding();
            lock = false;
            page++;
            $("#iconLoading").hide();
            if (window.location.hash != null && $(window.location.hash).length > 0)
                $('html,body').scrollTop($(window.location.hash).offset().top + 170);
        });
    }
}
function LoadContests() {
    if ($("#lstContests").length > 0) {
        $.getJSON("/Contest/GetContests", {
            page: page,
            format: format,
            rnd: Math.random()
        }, function (contests) {
            if (page == 0 && contests.length == 0) {
                $("#lstContests").removeClass("shadow");
                $("#lstContests").append('<p>暂无比赛</p>');
            }
            if (contests.length == 0) { $("#iconLoading").hide(); lock = true; return; }//尾页锁定
            for (var i in contests) {
                $("#lstContests").addClass("shadow");
                $("#lstContests").append('<div class="post-item post-item-zip">'
                                                 + '    <div class="post-title"><h3><a href="/Contest/' + contests[i].ID + '">' + contests[i].Title + '</a></h3></div>'
                                                 + '    <div class="post-info">' + contests[i].ProblemCount + ' Problem(s), Format: ' + contests[i].Format + ', Duration: ' + contests[i].TimeLength + ' / Start: ' + contests[i].Time + '</div>'
                                                 + '</div>');
            }
            lock = false;
            page++;
            $("#iconLoading").hide();
        });
    }
}
function LoadStatuses() {
    if ($("#lstStatuses").length > 0) {
        $.getJSON("/Status/GetStatuses", {
            page: page,
            nickname: nickname,
            result: result,
            contest_id: contest_id,
            problem_id: problem_id,
            rnd: Math.random()
        }, function (statuses) {
            if (page == 0 && statuses.length == 0) {
                $("#lstStatuses").removeClass('shadow');
                $("#lstStatuses").append('<p>没有符合条件的结果</p>');
            }
            if (statuses.length == 0) { $("#iconLoading").hide(); lock = true; return; }//尾页锁定
            for (var i = statuses.length - 1; i >= 0; i--) {
                BuildStatus(statuses[i]);
            }
            lock = false;
            page++;
            $("#iconLoading").hide();
        });
    }
}
function LoadSolutionTags()
{
    if ($("#lstSolutionTags").length > 0)
    {
        $.getJSON("/Solution/GetTags/" + id, {}, function (tags) {
            for (var i = 0; i < tags.length;i++)
            {
                $("#t_" + tags[i]).removeClass("gray");
                $("#t_" + tags[i]).addClass("orange");
            }
        });
    }
}

var StatusCss = ["ac", "pe", "wa", "ole", "tle", "mle", "rte", "ce", "se", "hacked", "running", "pending","hidden"];
var StatusDisplay = ["Accepted", "Presentation Error", "Wrong Answer", "Output Limit Exceeded", "Time Limit Exceeded", "Memory Limit Exceeded", "Runtime Error", "Compile Error", "System Error", "Hacked", "Running", "Pending","Hidden"];

function BuildStatus(status)
{
    var html = '<a id=s_' + status.ID + ' class="status-item" href="/Status/' + status.ID + '">'
                  + '<div class="status-item-wrap">'
                  + '<div class="status-face">' + status.Gravatar + '</div>'
                  + '<div class="status-cont"><div class="status-name"><h2>' + status.Nickname + '</h2></div></div>'
                  + '<div class="status-info">' + status.ProblemTitle + ' @' + status.TimeTip + '</div>'
                  + '<div class="status-status">' + status.Result + (status.PointCount > 1 ? ' (' + status.Statistics[0] + '/' + status.PointCount + ')' : "") + '</div>'
                  + '</div><div class="status-points">';
    var per = parseInt(100 / status.PointCount);
    var fill = 100 - per * (status.PointCount);
    var filled = false;
    for (var i = 0; i < status.Statistics.length; i++)
    {
        var p = per * status.Statistics[i];
        if (!filled && status.Statistics[i] > 0) {
            p += fill;
            filled = true;
        }
        if (status.Statistics[i] > 0)
            html += '<div class="status-point-item status-point-' + StatusCss[i] + '" style="width:' + p + '%"><div class="status-point-desc">' + StatusDisplay[i] + (status.PointCount > 1 ? ' : ' + status.Statistics[i] : "") + '</div></div>';
    }
    html += '</div></a>';
    if ($("#s_" + status.ID).length > 0) {
        $("#s_" + status.ID).html(html);
    }
    else {
        $("#lstStatuses").prepend(html);
    }
}
function BuildStatusDetail(status) {
    var html = '<div id=s_' + status.ID + ' class="status-item">'
                  + '<div class="status-item-wrap">'
                  + '<div class="status-face">' + status.Gravatar + '</div>'
                  + '<div class="status-cont"><div class="status-name"><h2><a href="/User/' + status.UserID + '">' + status.Nickname + '</a></h2></div></div>'
                  + '<div class="status-info"><a href="/Problem/' + status.ProblemID + '">' + status.ProblemTitle + '<a/> @' + status.TimeTip + '</div>'
                  + '<div class="status-status">' + status.Result + (status.PointCount > 1 ? ' (' + status.Statistics[0] + '/' + status.PointCount + ')' : "") + '</div>'
                  + '</div><div class="status-points">';
    var per = parseInt(100 / status.PointCount);
    var fill = 100 - per * (status.PointCount);
    var filled = false;
    for (var i = 0; i < status.Statistics.length; i++)
    {
        var p = per * status.Statistics[i];
        if (!filled && status.Statistics[i] > 0) {
            p += fill;
            filled = true;
        }
        if (status.Statistics[i] > 0)
            html += '<div class="status-point-item status-point-' + StatusCss[i] + '" style="width:' + p + '%"><div class="status-point-desc">' + StatusDisplay[i] + (status.PointCount > 1 ? ' : ' + status.Statistics[i] : "") + '</div></div>';
    }
    html += '</div></div>';
    if ($("#s_" + status.ID).length > 0) {
        $("#s_" + status.ID).html(html);
    }
    else {
        $("#lstStatuses").append(html);
    }
}

$(document).ready(function () {
    $("#iconLoading").hide();
    Load();
    $(window).scroll(function () {
        totalheight = parseFloat($(window).height()) + parseFloat($(window).scrollTop());
        if ($(document).height() <= totalheight) {
            Load();
        }
    });
});