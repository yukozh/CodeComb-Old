var key2desc = false;
var IsPMOpened = false;
var editor;
var CurrentContactID = null;
var RealTimeStatusID = null;
var status_id = null;
var hack_id = null;
var CodeCombHub;

function BroadCastBox()
{
    $.colorbox({
        inline: true,
        href: '#BroadCastBox',
        height: '475px',
        width:'700px',
        onComplete: function () {
            CKEDITOR.replace('txtBroadCast', { toolbar: 'Basic', width: '645px', height: '200px' });
        },
        onClosed: function () {
            CKEDITOR.instances.txtBroadCast.destroy();
        }
    });
}

function GetContacts()
{
    var contact_ids = GetCookie();
    $.getJSON("/PrivateMessage/GetContacts", { ids: JSON.stringify(contact_ids), rnd: Math.random() }, function (contacts) {
        var html = "";
        for (var i in contacts)
        {
            AutoAddContact(contacts[i].ID);
            html += '<p><a href="javascript:GetChatRecords(' + contacts[i].ID + ')">' + contacts[i].Gravatar + contacts[i].Nickname + '</a> ' + (contacts[i].MessageCount > 0 ? ('(' + contacts[i].MessageCount + ')') : '') + '</p>';
        }
        $("#lstContacts").html(html);
    });
}

function AddContact(id)
{
    var contact_ids = GetCookie();
    var existed = false;
    for (var i in contact_ids) {
        if (contact_ids[i] == id) {
            existed = true;
            break;
        }
    }
    if (!existed)
    {
        contact_ids.push(id);
        SetCookie(contact_ids);
    }
    GetContacts();
    GetChatRecords(id);
    PrivateMessageDispay();
}

function AutoAddContact(id)
{
    var contact_ids = GetCookie();
    for (var i in contact_ids)
    {
        if (id == contact_ids[i])
            return true;
    }
    contact_ids.push(id);
    SetCookie(contact_ids);
    return false;
}

function GetCookie()
{
    var tmp = $.cookie("c_" + username);
    if (tmp == null) tmp = "";
    var contact_ids = tmp.split(',');
    return contact_ids;
}

function SetCookie(ids)
{
    var ret = "";
    for (var i = 0; i < ids.length; i++)
        if (parseInt(ids[i]) > 0)
            ret += ids[i] + ",";
    ret = ret.substr(0, ret.length - 1);
    $.cookie("c_" + username, ret, { path: "/" });
}

function PrivateMessageDispay()
{
    IsPMOpened = true;
    $.colorbox({ inline: true, width: "auto", href: "#PrivateMessage", onClosed: function () { CurrentContactID = null; IsPMOpened = false; } });
    $("#btnPMDisplay").removeClass("btn-highlight");
    if (CurrentContactID == null) { 
        $("#PMRightArea").hide();
        $("#ChatRecords").html("");
    }
}

function GetChatRecords(sender_id)
{
    CurrentContactID = sender_id;
    $.getJSON("/PrivateMessage/GetChatRecords", { sender_id: sender_id, rnd: Math.random() }, function (chatrecords) {
        for (var i in chatrecords)
        {
            var html = '<dt>' + chatrecords[i].Gravatar + ' <a href="/User/' + chatrecords[i].SenderID + '">' + chatrecords[i].SenderNickname + '</a> ' + chatrecords[i].Time + ' :</dt><dd>' + chatrecords[i].Content + '</dd>';
            if ($("#cr_" + chatrecords[i].ID).length > 0)
                $("#cr_" + chatrecords[i].ID).html(html);
            else
            {
                html = '<dl id="cr_' + chatrecords[i].ID + '">' + html + '</dl>';
                $("#ChatRecords").append(html);
            }
        }
        $("#ChatRecords").show();
        $("#PMRightArea").show();
        document.getElementById('ChatRecords').scrollTop = document.getElementById('ChatRecords').scrollHeight;
        GetContacts();
    });
}

function PostMessage()
{
    $.post("/PrivateMessage/PostMessage", { receiver_id: CurrentContactID, content: $("#txtMessageContent").val(), rnd: Math.random() }, function () { $("#txtMessageContent").val(""); });
}

function ShowClar(id)
{
    $.getJSON("/Contest/GetClar/" + id, { rnd: Math.random() }, function (clar) {
        var title, question, answer, clarid;
        title = '<span style="color:blue">' + clar.ProblemRelation + "</span>";
        if (!clar.NoProblemRelation)
            title = "Problem: " + title;
        clarid = clar.ID;
        question = clar.Question;
        answer = clar.Answer;
        var html = '<h2>Judge\'s Response</h2>'
                      + '<p><strong>' + title + '</strong></p>'
                      + '<p><strong>Clar Id: <span style="color:blue">' + clarid + '</span></strong></p>'
                      + '<p><strong>Question: <span style="color:blue">' + question + '</span></strong></p>'
                      + '<p><strong>Answer: <span style="color:blue">' + answer + '</span></strong></p>';
        $.colorbox({ html: html, width: '700px' });
    });
}

function ClarResponse(id)
{
    $.getJSON("/Contest/GetClar/" + id, { rnd: Math.random() }, function (clar) {
        var title, question, answer, clarid;
        title = '<span style="color:blue">' + clar.ProblemRelation + "</span>";
        if (!clar.NoProblemRelation)
            title = "Problem: " + title;
        clarid = clar.ID;
        question = clar.Question;
        answer = clar.Answer;
        var html = '<h2>Judge\'s Response</h2>'
                      + '<p><strong>' + title + '</strong></p>'
                      + '<p><strong>Clar Id: <span style="color:blue">' + clarid + '</span></strong></p>'
                      + '<p><strong>Question: <span style="color:blue">' + question + '</span></strong></p>'
                      + '<p><strong>Answer: </strong></p>'
                      + '<p><textarea id="txtClarResponse" style="width:620px;" class="textbox"></textarea></p>'
                      + '<p><input id="btnClarResponse" type="button" class="button button-def" value="提交" /> <input type="checkbox" id="chkBroadcast"" />Broadcast</p>'
                      + '<input type="hidden" id="clar_id" cid="' + clarid + '" />';
        $.colorbox({
            html: html, width: '700px', onComplete: function () {
                $("#btnClarResponse").unbind().click(function () {
                    $.post("/Contest/ResponseClar/" + $("#clar_id").attr("cid"), { answer: $("#txtClarResponse").val(), broadcast: $("#chkBroadcast").is(':checked'), rnd: Math.random() }, function () {
                        $.colorbox.close();
                    });
                });
            }
        });
    });
}

function SetSolutionTag(tid)
{
    $.post("/Solution/SetTag/" + id, { tid: tid, rnd: Math.random() }, function (data) {
        if (data == "Added")
        {
            $("#t_" + tid).removeClass("gray");
            $("#t_" + tid).addClass("orange");
        }
        else if (data == "Deleted")
        {
            $("#t_" + tid).removeClass("orange");
            $("#t_" + tid).addClass("gray");
        }
    });
}

function EditTestCase(id)
{
    $.getJSON("/Problem/GetTestCase/" + id, { rnd: Math.random() }, function (testcase) {
        $("#txtInput").val(testcase.Input);
        $("#txtOutput").val(testcase.Output);
        $("#EditID").val(testcase.ID);
        $.colorbox({ inline: true, width: 'auto', href: '#TestCaseEdit' });
    });
}

function GetDetails(id) {
    $.getJSON("/Status/GetStatusDetails/" + id, { rnd: Math.random() }, function (details) {
        var html = "";
        for (var i in details) {
            html += '<h3><a href="javascript:void(0)" class="btnDetail" did="' + details[i].ID + '">#' + details[i].ID + ': <span class="status-text-' + StatusCss[details[i].Result] + '">' + StatusDisplay[details[i].Result] + '</span> (' + details[i].TimeUsage + 'ms, ' + details[i].MemoryUsage + 'KiB)</a></h3>';
            html += '<div class="status-detail-main" style="display:none" id="d_' + details[i].ID + '"><blockquote>';
            html += details[i].Hint;
            html += '</blockquote></div></div>';
        }
        $("#lstDetails").html(html);
        $(".btnDetail").unbind().click(function () {
            var did = $(this).attr("did");
            $("#d_" + did).toggle();
        });
    });
}

function HackResultDisplay(hack)
{
    var html = '<h3>评测结果</h3><table><tr><td style="text-align:center"><p><img src="' + hack.HackerGravatar + '" class="hack-avatar"></p><p>进攻方: <a href-"/User/' + hack.HackerID + '">' + hack.HackerName + '</a></p></td><td style="text-align:center"><h2>VS</h2>';
    html += '<h3 class="' + hack.Css + '">' + hack.Result + '</h3>';
    html += '<p>记录号: ' + hack.StatusID + '</p>';
    html += '<p>题目: <a href="/Problem/' + hack.ProblemID + '">' + hack.ProblemTitle + '</a></p>';
    html += '<p>进攻方: <a href-"/User/' + hack.HackerID + '">' + hack.HackerName + '</a></p>';
    html += '<p>防守方: <a href-"/User/' + hack.DefenderID + '">' + hack.DefenderName + '</a></p></td>';
    html += '<td style="text-align:center"><p><img src="' + hack.DefenderGravatar + '" class="hack-avatar"></p><p>防守方: <a href-"/User/' + hack.DefenderID + '">' + hack.DefenderName + '</a></p></td></tr></table>';
    $.colorbox({ html: html, width: '700px' });
}

$(document).ready(function () {
    //SignalR
    CodeCombHub = $.connection.codeCombHub;
    CodeCombHub.client.onStatusChanged = function (status) {
        if ($("#lstStatuses").length > 0) {
            if ($("#s_" + status.ID).length > 0) {
                BuildStatus(status);
            }
        }
        else {
            if ($("#s_" + status.ID).length > 0) {
                BuildStatusDetail(status);
                $("#MemoryUsage").html(status.MemoryUsage);
                $("#TimeUsage").html(status.TimeUsage);
                GetDetails(status.ID);
            }
        }
        if (RealTimeStatusID != null) {
            if (RealTimeStatusID != status.ID) return;
            var html_detail = "";
            $.getJSON("/Status/GetStatusDetails/" + status.ID, { rnd: Math.random() }, function (details) {
                for (var i in details) {
                    html_detail += '<p><a href="javascript:void(0)" class="btnDetail" did="' + details[i].ID + '">#' + details[i].ID + ': <span class="status-text-' + StatusCss[details[i].Result] + '">' + StatusDisplay[details[i].Result] + '</span> (' + details[i].TimeUsage + 'ms, ' + details[i].MemoryUsage + 'KiB)</a></p>';
                    html_detail += '<div class="status-detail-main" style="display:none" id="d_' + details[i].ID + '"><blockquote>';
                    html_detail += details[i].Hint;
                    html_detail += '</blockquote></div></div>';
                }
                $.colorbox({ html: '<h3>评测结果</h3><p><span class=status-text-' + StatusCss[status.ResultAsInt] + '>' + status.Result + '</span> Time=' + status.TimeUsage + 'ms, Memory=' + status.MemoryUsage + 'KiB</p><div id="lstDetails">' + html_detail + '</div>', width: '700px' });
                $(".btnDetail").unbind().click(function () {
                    var did = $(this).attr("did");
                    $("#d_" + did).toggle();
                    $.colorbox.resize('Height:auto');
                });
            });
        }
    };
    CodeCombHub.client.onStatusCreated = function (status) {
        if ($("#lstStatuses").length > 0) {
            if (contest_id != null && status.ContestID != contest_id) return;
            if (nickname != null && status._Nickname.indexOf(nickname) < 0) return;
            if (problem_id != null && problem_id != 0 && status.ProblemID != problem_id) return;
            BuildNewStatus(status);
        }
    };
    CodeCombHub.client.onMessageReceived = function (msg) {
        GetContacts();
        if (CurrentContactID != null)
            GetChatRecords(CurrentContactID);
        if (!IsPMOpened) $("#btnPMDisplay").addClass("btn-highlight");
    };
    CodeCombHub.client.onClarificationsResponsed = function (clar) {
        var title, question, answer, clarid;
        title = '<span style="color:blue">' + clar.ProblemRelation + "</span>";
        if (!clar.NoProblemRelation)
            title = "Problem: " + title;
        clarid = clar.ID;
        question = clar.Question;
        answer = clar.Answer;
        var html = '<h2>Judge\'s Response</h2>'
                      + '<p><strong>' + title + '</strong></p>'
                      + '<p><strong>Clar Id: <span style="color:blue">' + clarid + '</span></strong></p>'
                      + '<p><strong>Question: <span style="color:blue">' + question + '</span></strong></p>'
                      + '<p><strong>Answer: <span style="color:blue">' + answer + '</span></strong></p>';
        $.colorbox({ html: html, width: '700px' });
    }
    CodeCombHub.client.onClarificationsRequested = function (id) {
        ClarResponse(id);
    }
    CodeCombHub.client.onStandingsChanged = function (tid, data) {
        if (tid == id) {
            StandingsUpdate(data);
        }
    }
    CodeCombHub.client.onHacked = function (hack) {
        HackResultDisplay(hack);
    }
    CodeCombHub.client.onHackFinished = function (hack) {
        HackResultDisplay(hack);
    }
    CodeCombHub.client.onBroadCast = function (data) {
        var html = "<h3>BroadCast</h3>" + data;
        $.colorbox({ html: html, width: '700px' });
    }
    CodeCombHub.client.onJudgerStatusChanged = function (judger) {
        var html = '<img class="post-face" src="' + judger.Gravatar + '" /><a href="/User/' + judger.ID + '">' + judger.Nickname + '</a> ' + judger.Motto + ' 负载：' + judger.Ratio + '% 状态：<span class="' + judger.Css + '">' + judger.Status + '</span>';
        if ($("#j_" + judger.ID).length > 0)
            $("#j_" + judger.ID).html(html);
        else
            $("#lstJudgers").prepend('<p id="j_' + judger.ID + '">' + html + '<p id="j_' + judger.ID + '">');
    }
    $.connection.hub.start().done(function () {
        if ($("#lstJudgers").length > 0) {
            CodeCombHub.server.joinJudgeList();
        }
    });

    $("#btnAddContact").click(function () {
        var uid = $(this).attr("uid");
        AddContact(uid);
    });
    $("#btnPostMessage").click(function () {
        PostMessage();
    });
    $("#txtMessageContent").keydown(function (e) {
        if (e.ctrlKey && e.which == 13)
            PostMessage();
    });

    $("#btnRejudge").click(function () {
        $.post("/Status/Rejudge/" + id, $("#frmRejudge").serialize(), function (ret)
        {
            CastMsg("正在进行重测...");
        });
    });

    $("#btnBroadCast").click(function () {
        var content = CKEDITOR.instances.txtBroadCast.getData();
        $("#txtBroadCast").val(content);
        $.post("/Shared/BroadCast", $("#frmBroadCast").serialize(), function (ret) {
            if (ret == "OK") CastMsg("广播成功！");
            $.colorbox.close();
        });
    });

    $("#btnLoadCodeEditBox").click(function () {
        $.colorbox({ inline: true, width: "700px", href: "#CodeEditBox", onComplete: function () { editor.refresh(); } });
    });
    function RefreshHighLight() {
        var language_id = $("#lstLanguages").val();
        switch (parseInt(language_id)) {
            case 0:
                editor.setOption('mode', 'text/x-csrc');
                break;
            case 1:
                editor.setOption('mode', 'text/x-c++src');
                break;
            case 2:
                editor.setOption('mode', 'text/x-c++src');
                break;
            case 3:
                eeditor.setOption('mode', 'text/x-java');
                break;
            case 4:
                editor.setOption('mode', 'text/x-pascal');
                break;
            case 5:
                editor.setOption('mode', 'text/x-python');
                break;
            case 6:
                editor.setOption('mode', 'text/x-python');
                break;
            case 7:
                editor.setOption('mode', 'text/x-ruby');
                break;
            case 8:
                editor.setOption('mode', 'text/x-csharp');
                break;
            case 9:
                editor.setOption('mode', 'text/x-vb');
                break;
            default: break;
        }
        editor.refresh();
    }
    if ($("#editor").length > 0)
    {
        editor = CodeMirror.fromTextArea(document.getElementById("editor"), {
            lineNumbers: true,
            matchBrackets: true,
            indentUnit: 4,
            smartIndent: false,
            mode: "text/x-c++src",
            theme: "neat"
        });
        RefreshHighLight();
        $("#lstLanguages").change(function () {
            RefreshHighLight();
        });
    }
    $("#btnClearCodeBox").click(function () {
        editor.setValue("");
    });

    $("#btnSubmitCode").click(function () {
        $.colorbox({ html: '<h3>评测结果</h3><p>正在等待系统验证及分配评测资源...</p>', width: '700px' });
        $("#editor").val(editor.getValue());
        $.post("/Status/Create", $("#frmSubmitCode").serialize(), function (data) {
            if (data == "Problem not existed")
            {
                CastMsg("不存在这道题目！");
                $.colorbox.close();
            }
            else if (data == "Insufficient permissions")
            {
                CastMsg("权限不足！");
                $.colorbox.close();
            }
            else if (data == "Locked") {
                CastMsg("锁定题目后不能提交！");
            }
            else if (data == "Wrong phase") {
                CastMsg("本阶段不允许提交评测！");
            }
            else if (data == "No Online Judger") {
                CastMsg("评测机离线！当有可用评测资源时将评测本记录。");
            }
            else if (data == "OI") {
                CastMsg("提交成功");
            }
            else {
                RealTimeStatusID = parseInt(data);
                $.colorbox({ html: '<h3>评测结果</h3><p>正在评测...</p>', width: '700px' });
            }
        });
    });

    // 代码高亮插件初始化
    if (navigator.userAgent.indexOf("MSIE") == -1) {
        hljs.initHighlightingOnLoad();
    }

    // 窄页面左侧说明文字
    $('.profile-btn').mouseenter(function () {
        $('.profile-btn-alt').remove();
        $(this).parent().append('<a href="javascript:;" class="profile-btn-alt shadow" style="position: absolute; top: ' + ($(this).prevAll().size() * 64) + 'px; left: -68px;">' + $(this).children().attr('alt') + '</a>')
        //alert($(this).children().attr('alt'));
    });

    $('.profile-btn').mouseleave(function () {
        $('.profile-btn-alt').remove();
    });

    $(".testcase-copywrap").zclip({
        path: "ZeroClipboard.swf",
        copy: function () {
            var t = $(this).data("id"),
                e = "__new__line__" + (new Date).getTime(),
                i = $("<div></div>").html($("#testcase-" + t).html().replace(/<br([\s\S]*?)>/gi, e));
            return i.text().replace(/\n/g, "\r\n").replace(/\r\r/g, "\r").replace(new RegExp(e, "gi"), "\r\n");
        },
        afterCopy: function () {
            CastMsg("已复制到剪贴板。");
        }
    });
});