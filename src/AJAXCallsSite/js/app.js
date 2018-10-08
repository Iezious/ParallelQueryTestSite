const BASE = "/pagemethods.aspx";

function init()
{
    let selNumFast = document.getElementById("fast-call-number");
    let selNumSlow = document.getElementById("slow-call-number");
    let selNumPost = document.getElementById("post-call-number");
    let selDelaySlow = document.getElementById("slow-call-delay");
    let selDelayPost = document.getElementById("post-call-delay");
    
    let btn = document.getElementById("go-button");
    let sessionButton = document.getElementById("set-session-button");
    let activeQueries = 0;

    function incActive()
    {
        activeQueries++;
        btn.disabled = true;
    }

    function decActive()
    {
        activeQueries--;
        if (activeQueries == 0)
            btn.disabled = false;
    }

    function repeat(count, fn)
    {
        for (let i = 0; i < count; i++) fn();
    }

    btn.onclick = function()
    {
        let reqno = (new Date()).getTime();

        repeat(selNumFast.value * 1, () =>
        {
            incActive();
            httpSend("/fast/", {reqno: reqno}, null, decActive, decActive);
        });

        let sdelay = selDelaySlow.value * 1;

        repeat(selNumSlow.value * 1, () =>
        {
            incActive();
            httpSend("/slow/", {reqno: reqno, wait: sdelay}, null, decActive, decActive);
        });

        let pdelay = selDelayPost.value * 1;

        repeat(selNumPost.value * 1, () =>
        {
            incActive();
            httpSend("/post/", { reqno: reqno }, { wait: pdelay }, decActive, decActive);
        });
    };

    sessionButton.onclick = function ()
    {
        let reqno = (new Date()).getTime();
        httpSend("/set/", { reqno: reqno }, null, () => {}, () => {});
    };
}


function httpSend(url, params, body, onsuccess, onerror)
{
    let xhr = new XMLHttpRequest();

    let vurl = BASE + url;
    if (params)
    {
        let query = [];
        for (let key in params)
            query.push(key + "=" + params[key]);

        vurl = vurl + "?" + query.join("&");
    }

    xhr.open(body ? "POST" : "GET", vurl, true);
    xhr.onreadystatechange = function()
    {
        if (xhr.readyState != 4) return;

        if (xhr.status == 200 || xhr.status == 304)
        {
            if (onsuccess) onsuccess(xhr.responseText);
        }
        else
        {
            if (onerror) onerror(xhr.status, xhr.responseText);
        }
    };

    xhr.send(body ? JSON.stringify(body) : null);
}

window.onload = init;