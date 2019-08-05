using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using dtSearch.Engine;


namespace dtSearch.Sample
{


    public class SampleFileConverter
    {

        public static string HighlightHits(SampleApp sampleApp, SearchResults results, int ordinalInResults, string webRoot)
        {
            // Set up option settings if needed.
            // Options are maintained per-thread, so this must be done
            // on the thread where the job will execute.
            sampleApp.InitOptions();
            return HighlightHits(results, ordinalInResults, webRoot);
        }


        public static string HighlightHits(SearchResults results, int ordinalInResults, string webRoot)
        {
            string errorMessage = null;

            using (FileConverter fc = new FileConverter())
            {
                if (SetupFileConverter(fc, results, ordinalInResults, webRoot))
                {
                    fc.Execute();
                    if (!fc.Failed())
                        return fc.OutputString;
                    else
                        errorMessage = fc.Errors.ToString();
                }
                else
                    errorMessage = "Error accessing document information in SearchResults";
            }
            return "<HTML><H3>" + errorMessage + "</H3>";
        }
        public static bool SetupFileConverter(FileConverter fc, SearchResults results, int ordinalInResults, string webRoot)
        {
            if (!fc.SetInputItem(results, ordinalInResults))
                return false;

            fc.DocTypeTag = "<!DOCTYPE html>";
            fc.Footer = "<hr><i>" + fc.InputFile + "</i>";
            // DocScript is used to implement hit navigation, and DocStyles defines certain
            // standard CSS styles to format the output.
            string script = DocScript.Replace("%HITCOUNT%", fc.Hits.Length.ToString());
            fc.HtmlHead = script + DocStyles;
            string baseRef = fc.InputFile;
#if NETCOREAPP2_0
            if (!baseRef.StartsWith("http://") && !baseRef.StartsWith("https://"))
            {
                baseRef = "file://" + baseRef;
                if (!string.IsNullOrEmpty(webRoot))
                {
                    string rel = Path.GetRelativePath(webRoot, fc.InputFile);
                    if (!rel.StartsWith(".."))
                    {
                        baseRef = rel.Replace(Path.DirectorySeparatorChar, '/');
                    }
                }
            }
#endif
            fc.BaseHRef = baseRef;
            fc.BeforeHit = "<span id=\"hit%%ThisHit%%\" style=\"background-color: #FFFF00;\">";
            fc.AfterHit = "</span>";
            fc.OutputToString = true;
            // Use the styles in DocStyles to format output
            fc.Flags = fc.Flags | ConvertFlags.dtsConvertUseStyles |
                    // Update search hits if the index is out of date
                    ConvertFlags.dtsConvertAutoUpdateSearch |
                    // Disable JavaScript in input HTML
                    ConvertFlags.dtsConvertRemoveScripts;
            fc.OutputFormat = OutputFormat.itHTML;
            return true;

        }
        // Modify FileConverter to highlight hits using multiple colors.
        // The FileConverter must have been previously set up with the
        // document to highlight.
        // The search must have been done with these flags: dtsSearchWantHitsByWord | dtsSearchWantHitsArray | dtsSearchWantHitsByWordOrdinals
        public static void SetupMulticolorHighlighting(FileConverter fc)
        {
            string[] colors = {
            "#ffff00", // yellow
            "#a6f500", // light green
            "#00ffe3", // light aqua
            "#cbe3ff", // light blue
            "#c5c3fa", // purple
            "#ffbcff", // pink
            "#ff9999", // salmon
            "#fcbf29", // orange 
            "#d7d7d7", // gray
            "#eaddc2" // beige
            };
            string delimiter = "|";
            string beforeHit = delimiter;
            foreach (string s in colors)
            {
                beforeHit += "<span id=\"hit%%ThisHit%%\" style=\"background-color: " + s + "\">" + delimiter;
            }
            fc.BeforeHit = beforeHit;
            fc.AfterHit = "|</span>";
            fc.Flags |= ConvertFlags.dtsConvertMultiHighlight;
        }
        private static string DocScript = @"
<script>
var firstHitDone;
var iHit = 0;
var hitCount = %HITCOUNT%;

    function nextHit() {
        if (iHit < hitCount)
    	    gotoNthHit(iHit+1);
        }
    function prevHit() {
    	if (iHit > 1) 
	        gotoNthHit(iHit-1);
        }

    function firstHit()
    {
        if (firstHitDone != 1) {
            firstHitDone = gotoNthHit(1);
            }
    }

    function getOffset(obj)
    {
        var offset = obj.offsetTop;
        while (obj.offsetParent)
        {
            obj = obj.offsetParent;
            offset += obj.offsetTop;
        }
        return offset;
    }

    function getScrollOffset()
    {
        // Browsers other than IE 8 and earlier
        if (window.pageYOffset != null)
            return window.pageYOffset;
        // IE standards mode
        if (document.compatMode == ""CSS1Compat"")
            return document.documentElement.scrollTop;
        // IE quirks mode
        return document.body.scrollTop;
    }

    function gotoNthHit(n) { 
        iHit = n; 
        if ((n > 1) && (n == hitCount)) 
            gotoHit('hit_last'); 
        else if (n == 0) 
            gotoHit('hit0'); 
        else 
            gotoHit('hit' + n); 
     } 

    function gotoHit(where)
    {
    

        try
        {
            var a = document.getElementById(where);
            if (a == null)
            {
                return -1;
            }
            if (a.length > 1)
            {
                return -2;
            }
		    a.scrollIntoView(false);

		    if (document.body.createTextRange) {
			    // IE
            var s = document.body.createTextRange();
			    if (s != null) {
            s.moveToElementText(a);
            s.moveEnd(""word"");
				    s.select();
				    }
                }
		    else {
			    var s = document.createRange();
			    if (s != null) {
				    s.selectNodeContents(a);
				    var sel = window.getSelection();
                    sel.removeAllRanges();
				    sel.addRange(s);
			    }
            }

            return 1;
        }
        catch (ex)
        {
        }
        return -4;
    }
</script>
";
        private static string DocStyles = @"
<style>
BODY {
			font-family: segoe ui, arial;
			font-size: 10pt;
		}

.dts-field-table  
	{ 		border: 1; 
			padding: 0; 
			margin-top: 2mm; 
			margin-bottom: 2mm; 
			background-color: #F0F8FF;  
			border-radius: 10px; 
			-webkit-border-radius: 10px; 	
			-moz-border-radius: 10px; 	
			border: 0; 
	}

.dts-field-table-value-cell  
	{ 		font-size: 10pt; 
			font-family: segoe ui, arial; 
			text-align: left;  
			width: 54em;
			vertical-align: top;
			border-bottom: 1px solid #fff;
			border-top: 1px solid transparent;
	}

.dts-field-table-name-cell  
	{ 		font-size: 10pt; 
			font-family: segoe ui, arial; 
			font-weight: bold; 
			text-align: right; 
			padding-right: 1em;
			width: 8em; 
			vertical-align: top;
			border-bottom: 1px solid #fff;
			border-top: 1px solid transparent;
	}

.dts-begin-attachment
	{ 		font-size: 14pt;
			font-family: segoe ui, arial; 
			font-weight: bold; 
			margin-top: 1em;
			margin-bottom: .5em;
			background-color: #F0F8FF;  
			padding-top: .3em;
			padding-bottom: .3em;
			padding-left: 2em;
			padding-right: 2em;
			
			border-radius: 10px; 
			-webkit-border-radius: 10px; 	
			-moz-border-radius: 10px; 	
			border: 0; 
			width: 62em;
	}			

.dts-begin-file
	{ 		font-size: 14pt;
			font-family: segoe ui, arial; 
			font-weight: bold; 
			margin-top: 1em;
			margin-bottom: .5em;
			background-color: #F0F8FF;  
			padding-top: .3em;
			padding-bottom: .3em;
			padding-left: 2em;
			padding-right: 2em;
			border-radius: 10px; 
			-webkit-border-radius: 10px; 	
			-moz-border-radius: 10px; 	
			border: 0; 
			width: 62em;
	}			

.dts-begin-worksheet
	{
 		font-size: 12pt;
			font-family: segoe ui, arial; 
			font-weight: bold; 
			margin-top: 1em;
			margin-bottom: .5em;
			background-color: #F0F8FF;  
			padding-top: .3em;
			padding-bottom: .3em;
			padding-left: 2em;
			padding-right: 2em;
			border-radius: 10px; 
			-webkit-border-radius: 10px; 	
			-moz-border-radius: 10px; 	
			border: 0; 
			width: 42em;		
		
	}
	
table.dts-worksheet-table
	{
		border: 1px;
		padding-top: .3em;
		padding-bottom: .3em;
	}		

p.dts-section-break 
	{	margin: 0 0 0 0;
		padding: 0 0 0 0;
		background-color: #F0F8FF;  
		display: block;	
		height: 2px;
	}	
	
sub { vertical-align:text-bottom; font-size:75%; }
sup { vertical-align:text-top; font-size:75%; }	 	
</style>
";

    }



}

