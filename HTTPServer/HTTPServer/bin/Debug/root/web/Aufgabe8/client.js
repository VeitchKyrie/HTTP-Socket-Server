var filter = ""

window.onload = function ()
{
    sendValues('GET', '', 'get', 'users');
};

function crudTypeChanged(select)
{
    var hiddenStyle = "display: none";
    var showStyle = "";

    switch(select.selectedIndex)
    {
        case 0:
            document.getElementById("createUser").style = showStyle;
            document.getElementById("updateUser").style = hiddenStyle;
            document.getElementById("deleteUser").style = hiddenStyle;
            document.getElementById("success").innerHTML = "";
            break;

        case 1:
            document.getElementById("createUser").style = hiddenStyle;
            document.getElementById("updateUser").style = showStyle;
            document.getElementById("deleteUser").style = hiddenStyle;
            document.getElementById("success").innerHTML = "";
            break;

        case 2:
            document.getElementById("createUser").style = hiddenStyle;
            document.getElementById("updateUser").style = hiddenStyle;
            document.getElementById("deleteUser").style = showStyle;
            document.getElementById("success").innerHTML = "";
            break;
    }
}

function filterTypeChanged(select)
{
    switch (select.selectedIndex)
    {
        case 0:
            filter = "";
            break;

        case 1:
            filter = "/username";
            break;

        case 2:
            filter = "/name";
            break;

        case 3:
            filter = "/birthdate";
            break;
    }

    sendValues('GET', '', 'get', 'users');
}

function sendValues(verb, content, rootverb, id)
{
    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function ()
    {
        if (this.status == 200 || this.status == 404)
        {
            document.getElementById(id).innerHTML = this.responseText;

            if ((this.status == 200 || this.status == 204) && id != "users")
                sendValues('GET', '', 'get', 'users');
            else if(id == "users" && document.getElementById(id).innerHTML.length < 5)
                document.getElementById(id).innerHTML = "No users found in database."
        }
    };

    xhttp.open(verb, "/api/user" + filter, true);
    xhttp.setRequestHeader("Content-type", "text/xml");

    xhttp.send(content);
    username.value = "";
    realname.value = "";
    birthdate.value = "";
    
    id.textContent = "User has been " + rootverb + "ed."
}

function goTo(url)
{
    window.open(url, "_self");
}