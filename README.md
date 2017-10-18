# PDF Utils

A Simple tool for day-to-day pdf work - Split pdfs, Add cover letters and footers, place watermarks, etc. You can even extract text/id from pages for prefix and suffix to rename files.

I tried to keep it simple for my users - Feature requests are welcome (in the Discussions section please)

1. You can now add a cover and footer document to the cutup
2. Added feature to search for name within the document for renaming
3. Limits length of file names to 200 chars


Resulting filename will be like this

PREFIXOriginalFileNameSearchText_filecount.pdf

PREFIX – this will appear only if you are searching for id separately.. otherwise this won’t be there

OriginalFilename – Filename of the input document

SearchText – Text found between tags {# and #}

FileCount – Based on your choses option it will either append cutup number or starting page number of cutup.. to maintain unique filenames. This will also indicates you the location within source document, which is required most of times to cross check.
