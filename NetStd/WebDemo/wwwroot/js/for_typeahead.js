// JavaScript to use with the Twitter Typeahead control.
// If this script is included on a search form, it will look for a #SearchRequest control 
// and add the Twitter Typeahead control as a dropdown as the user types a search request.


//
// Typeahead replaces the whole query value so if the request has more than one
// word, all of the words except the last will be lost when a word is selected.
// 
// To work around this, save each request as a word list is generated from it
// in wordSource and when a select event occurs, use the saved request to
// restore the words other than the last one
var prevQuery;

function onTypeaheadSelect(evt, word, sourceName) {
    if (!prevQuery.includes(" "))
        return;
    var result = "";
    var words = prevQuery.split(" ");
    if (words.length < 2)
        return;
    // If the original query included multiple words, rebuild it replacing only the last
    // word with the typeahead word
    for (i = 0; i < words.length - 1; ++i)
        result = result + words[i] + " ";
    result = result + word;

    // This invokes the 'val' method of the typeahead to assign a new value.
    // See https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#jquerytypeaheadval-val
    $(this).typeahead('val', result);
    console.log("Selected " + word + " query=" + prevQuery + " result=" + result);
}


function onTypeaheadAutocomplete(evt, word, sourceName) {
    console.log("Autocomplete " + word + " query=" + prevQuery);
}

// This connects the typeahead control to the dtSearch WordListBuilder via an http 
// query of the form: http://example.com/api/words/list?ixid=1&type=0&ct=100&req=search-request
function wordSource(query, syncCallback, asyncCallback) {
    // Save the query so it can be restored in onTypeaheadSelect 
    prevQuery = query;

    // URL to request words from the server.
    // This will be handled by the WordsController class, which will
    // return an array of words

    // The rootURL is normally /, but the web server may have URL Rewriting rules that change this.
    // The search form has a dummy href with id "RootUrl" that we can use here to detect
    // any URL rewriting that has occurred.
    var rootURL = $('#RootUrl').attr('href');
    if (rootURL == null)
        rootURL = '/';

    var queryURL = rootURL + 'api/words/list?ixid=' + getIxId() + '&type=0&ct=100&req=' + encodeURIComponent(query);

    // Log the query and the URL to the console
    console.log(query + "=>" + queryURL);

    // Make the AJAX call using JQuery, and call the async callback with the results
    $.getJSON(queryURL, null, function (data, status) {
        asyncCallback(data);
    });
}

// Get the IndexId of the index to use for the Typeahead control.
//
// The IndexId is an integer that maps to the index path using a table in 
// the AppSettings class.
//
function getIxId() {
    var indexControls = $("[name='IxId']");
    var ret = "";
    indexControls.each(function () {
        if ((this.type == 'hidden') || (this.checked)) {
            ret = this.value;
            return false;
        }
    });

    // If no indexes are selected, use the first one on the form for the word list
    if ((ret.length == 0) && (indexControls.length > 0))
        ret = indexControls[0].value;
    return ret;
}

function setSearchRequest(v) {
    $('#SearchRequest').typeahead('val', v);
}

$(document).ready(function () {
    var req = $('#SearchRequest');
    req.typeahead(
        // Options
        {
            hint: true,
            highlight: true,
            minLength: 1
        },
        // Dataset
        {
            name: 'WordListBuilderDemo',
            limit: 1000,
            source: wordSource
        }).on("typeahead:selected", onTypeaheadSelect)
        .on("typeahead:autocompleted", onTypeaheadAutocomplete);
    });