// JavaScript to enable a scrolling word list in the search form
//
// If this script is added to a form, it will look for a #WordList div
// and a #SearchRequest and will update the WordList with a scrolling
// list of words similar to what dtSearch Desktop displays as the user
// types in the #SearchRequest input.

// Number of words before and after the search term to request
var WordRange = 6;

// Used to prevent overlapping queries
var iQuery = 0;
var lastCompletedQuery = 0;
var pendingQuery = null;

// After a search, the word list is intially closed and should automatically open
// if the user starts typing.  This flag is used to control this behavior.
var bAutoOpenWordList = false;

function getWords(query) {
    if (!$('#WordList').is(":visible")) {
        console.log("The WordList is not visible");
        return;
        }
    if (query.length == 0)
        query = "a";

    // This prevents a backlog of queries if network latency or server response time causes
    // the word list to fail to keep up with the user's typing.  If there is already a query
    // in progress when a new query arrives, save it to be processed after the previous query
    // is done.  This way if 5 queries arrive before a query is processed, only the last one
    // will be sent to the server.
    if (iQuery > lastCompletedQuery + 1) {
        pendingQuery = query;
        console.log("Deferring: " + query);
        return;
    }

    // Get the URL to request words from the server.
    // This will be handled by the WordsController class, which will return an array of words and counts

    // The rootURL is normally /, but the web server may have URL Rewriting rules that change this.
    // The search form has a dummy href with id "RootUrl" that we can use here to detect
    // any URL rewriting that has occurred.
    var rootURL = $('#RootUrl').attr('href');
    if (rootURL == null)
        rootURL = '/';

    var queryURL = rootURL + 'api/words/info?ixid=' + getIxId() + '&type=1&ct=' + WordRange + '&req=' + encodeURIComponent(query);
    var thisQueryNum = iQuery++;

    // Log the query and the URL to the console
    console.log("#" + thisQueryNum + ": Sending " + query + "=>" + queryURL);

    // Make the AJAX call using JQuery, and call the async callback showWords with the results
    $.getJSON(queryURL, null, function(data, status) {
        showWords(thisQueryNum, data);
        });
}

function openWordList() {
    $('#WordList').collapse('show');
    var button = $('#WordListButton').find("span");
    button.removeClass('glyphicon-collapse-up');
    button.addClass('glyphicon-collapse-down');
}


function onKey() {
    if (bAutoOpenWordList) {
        openWordList();
        bAutoOpenWordList = false;
    }
    updateWordList();
}

function updateWordList() {
    getWords($('#SearchRequest').val());
}

function onIndexChanged() {
    console.log("Index selection changed");
    updateWordList();
}

// The word list is not populated when hidden, so if the user clicks the button to show the
// word list, populate the word list so there will be something to show.
function onShowWordList() {
    console.log("The word list opened");
    updateWordList();
}

// showWords builds an HTML table inside the #WordList div to display the scrolling list of 
// words as the user enters the search request
function showWords(thisQueryNum, data) {
    var div = $('#WordList');
    div.empty();
    var s = "<table class='wordlist'>";

    $.each(data, function(index, value) {
        if (index > 2 * WordRange)
            return false;
        var sel = (index == WordRange ? " wordlistsel" : "");
        var count = value.count;
        var countFmt = count.toLocaleString();
        if (count == 0)
            countFmt = "&nbsp;";
        s = s + ("<tr class=\"wordlistrow" + sel + "\"><td class=\"wordlistitemcount\">"
            + countFmt + "</td><td class=\"wordlistitem\">" + value.word + "</td></tr>");
        });
    div.append("</table>");
    div.append(s);
    console.log("#" + thisQueryNum + ": Done ");
    lastCompletedQuery = Math.max(lastCompletedQuery, thisQueryNum);

    if (pendingQuery != null) {
        var toShow = pendingQuery;
        pendingQuery = null;
        console.log("Resuming pending query: " + toShow);
        getWords(toShow);
        }
}

// Get the IndexId of the index to use for the WordList control.
//
// The IndexId is an integer that maps to the index path using a table in 
// the AppSettings class.
//
// If the search form provides only one index, a hidden input control will
// specify the IndexId.  If the search form provides multiple IxId checkboxes
// then getIxId will find the first one that is checked.
function getIxId() {
    var indexControls = $("[name='IxId']");
    var ret = "";
    indexControls.each(function () {
        if ((this.type == 'hidden') || (this.checked))  {
            ret = this.value;
            return false;
            }
    });

    // If no indexes are selected, use the first one on the form for the word list
    if ((ret.length == 0) && (indexControls.length > 0))
        ret = indexControls[0].value;
    return ret;
}

// Set up the search controls to update the word list whenever a key is pressed or the
// checked state of any of the IndexId checkboxes changes
$(document).ready(function () {
    var req = $('#SearchRequest');
    req.keyup(onKey);

    if (req.val().length == 0)
        getWords("a");
    else
        bAutoOpenWordList = true;

    // Update the word list each time the index selections change
    var indexControls = $("[name='IxId']");
    indexControls.each(function () {
        this.onchange = onIndexChanged;
    });
    // Toggle the icon on WordList collapse up/down button
    $('#WordListButton').click(function () {
        $(this).find("span").toggleClass('glyphicon-collapse-down').toggleClass('glyphicon-collapse-up');
    });

    // Update the word list when it is made visible
    $('#WordList').on('shown.bs.collapse', onShowWordList);

    });

