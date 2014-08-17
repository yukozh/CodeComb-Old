var IsPMOpened = false;
var editor;
var CurrentContactID = null;

function GetContacts()
{
    var contact_ids = GetCookie();
    $.getJSON("/PrivateMessage/GetContacts", { ids: JSON.stringify(contact_ids) }, function (contacts) {
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
    $.getJSON("/Contest/GetClar/" + id, {}, function (clar) {
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
    $.getJSON("/Contest/GetClar/" + id, {}, function (clar) {
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
                    $.post("/Contest/ResponseClar/" + $("#clar_id").attr("cid"), { answer: $("#txtClarResponse").val(), broadcast: $("#chkBroadcast").is(':checked') }, function () {
                        $.colorbox.close();
                    });
                });
            }
        });
    });
}

$(document).ready(function () {
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
    //setInterval(function () {
    //    GetContacts();
    //    if (CurrentContactID != null)
    //        GetChatRecords(CurrentContactID);
    //}, 5000);

    //SignalR
    var CodeCombHub = $.connection.codeCombHub;
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
                GetDetails();
            }
        }
    };
    CodeCombHub.client.onStatusCreated = function (status) {
        if ($("#lstStatuses").length > 0)
        {
            if (contest_id != null && status.ContestID != contest_id) return;
            if (nickname != null && status._Nickname.indexOf(nickname) < 0) return;
            if (problem_id != null && problem_id != 0 && status.ProblemID != problem_id) return;
            BuildStatus(status);
        }
    };
    CodeCombHub.client.onMessageReceived = function (msg) {
        GetContacts();
        if (CurrentContactID != null)
            GetChatRecords(CurrentContactID);
        if (!IsPMOpened) $("#btnPMDisplay").addClass("btn-highlight");
    };
    CodeCombHub.client.onClarificationsResponsed = function (clar)
    {
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
    $.connection.hub.start();

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
        $.colorbox({ html: '<h3>评测结果</h3><p>正在等待系统验证及分配评测资源...</p>', width:'700px' });
        $.post("/Status/Create", $("#frmSubmitCode").serialize(), function (data) {
            if (data == "Problem not existed")
                $.colorbox({ html: '<h3>评测结果</h3><p>不存在这道题目！</p>', width: '700px' });
            else if(data=="Insufficient permissions")
                $.colorbox({ html: '<h3>评测结果</h3><p>权限不足！</p>', width: '700px' });
            else if (data == "Locked")
                $.colorbox({ html: '<h3>评测结果</h3><p>您已锁定了该题，无法再次提交！</p>', width: '700px' });
            else if (data == "Wrong phase")
                $.colorbox({ html: '<h3>评测结果</h3><p>本阶段不允许提交！</p>', width: '700px' });
            else
                alert(data);
        });
    });
});