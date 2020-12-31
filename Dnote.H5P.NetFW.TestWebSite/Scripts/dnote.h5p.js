/**
 * Implementation of the h5p-content web component.
 * Possible attributes are:
 * content-id - Required. Id of the H5P content item. 
 * title - Title to show inside the content.
 * css - Comma seperated list of css files that are required by this content.
 * js - Comma seperated list of js files that are required by this content.
 * library - Formatted name and version of the library of the content.
 * jsonContent - Content itself.
 * userContent - Optional. The answer the user submitted previously.
 * prefix - Prefix url for the files referenced by the contnent.
 * api-url-prefix - Optional. Url prefix of api calls to store user content and content state.
 * auto-init - Optional. If set to true, the content is automatically initialized (the default), else the using code has to call init() to initialize the content.
 **/
class H5PContent extends HTMLElement
{
    constructor()
    {
        super();
    }

    connectedCallback()
    {
        let autoInit = true;
        if (this.getAttribute('auto-init'))
        {
            autoInit = this.getAttribute('auto-init') === 'true';
        }

        if (autoInit)
        {
            this.init();
        }
    }

    init() 
    {
        const _this = this;
        const contentId = this.getAttribute('content-id');
        const title = this.getAttribute('title');
        const css = this.getAttribute('css').split(',');
        const js = this.getAttribute('js').split(',');
        const library = this.getAttribute('library');
        const jsonContent = this.getAttribute('json-content');
        const userContent = this.getAttribute('user-content');
        const autoShowCompleted = this.getAttribute('auto-show-completed') ?? true;
        const prefix = this.getAttribute('prefix');
        let apiUrlPrefix = this.getAttribute('api-url-prefix');
        if (apiUrlPrefix && apiUrlPrefix.endsWith('/'))
        {
            apiUrlPrefix = apiUrlPrefix.substr(0, apiUrlPrefix.length - 1);
        }

        // Below is the h5p content item placeholder which is replace by the h5p system.
        this.innerHTML = `<div class="h5p-content" id="${contentId}" data-content-id="${contentId}"></div>`;

        if (!window.H5PCompleted)
        {
            window.H5PCompleted = {};
        }

        window.H5PCompleted[contentId] = userContent !== null;

        if (!window.H5PAutoShowCompleted)
        {
            window.H5PAutoShowCompleted = {};
        }

        window.H5PAutoShowCompleted[contentId] = autoShowCompleted;

        window.H5PIntegration.url = prefix;
        window.H5PIntegration.postUserStatistics = !!apiUrlPrefix;
        window.H5PIntegration.ajaxPath = '/' + apiUrlPrefix;
        window.H5PIntegration.ajax.setFinished = apiUrlPrefix ? '/' + apiUrlPrefix + '/SetFinished' : null;
        window.H5PIntegration.ajax.contentUserData = apiUrlPrefix ? '/' + apiUrlPrefix + '/UserData/:contentId?data_type=:dataType&subContentId=:subContentId' : null;

        window.H5PIntegration.contents['cid-' + contentId] = {
            'library': library, // Library name + major version.minor version
            'jsonContent': jsonContent,
            'fullScreen': false, // No fullscreen support
            //'exportUrl': '/path/to/download.h5p',
            'mainId': 1234,
            'url': 'https://mysite.com/h5p/1234',
            'title': title,
            'contentUserData': {
                0: { // Sub ID (0 = main content/no id)
                    'state': userContent ?? '{}'
                }
            },
            'displayOptions': {
                'frame': true, // Show frame and buttons below H5P
                'export': true, // Display download button
                'embed': true, // Display embed button
                'copyright': true, // Display copyright button
                'icon': true // Display H5P icon
            }
        };

        if (!window.DnoteH5PCss)
        {
            window.DnoteH5PCss = [];
        }

        for (var i = 0; i < css.length; i++)
        {
            const cssFile = css[i];
            if (window.DnoteH5PCss.indexOf(cssFile) === -1)
            {
                var fileref = document.createElement("link");
                fileref.setAttribute("rel", "stylesheet");
                fileref.setAttribute("type", "text/css");
                fileref.setAttribute("href", cssFile);
                document.head.appendChild(fileref);
                window.DnoteH5PCss.push(cssFile);
            }
        }

        if (!window.DnoteH5PJs)
        {
            window.DnoteH5PJs = [];
        }

        const lastJs = js.length > 0 ? js[js.length - 1] : null;

        function createScriptElement()
        {
            // gets the first script in the list
            let script = js.shift();
            // all scripts were loaded
            if (!script) return;

            if (window.DnoteH5PJs.indexOf(script) === -1)
            {
                let jsfileref = document.createElement('script');
                jsfileref.type = 'text/javascript';
                jsfileref.src = script;
                jsfileref.onload = function (event)
                {
                    // loads the next script
                    createScriptElement();

                    if (event.srcElement.getAttribute('src') === lastJs)
                    {
                        // Init the H5P system after the last javascript is loaded.
                        H5P.init(_this);
                    }
                };
                document.body.append(jsfileref);
                window.DnoteH5PJs.push(script);
            }
            else
            {
                if (script === lastJs)
                {
                    // Init the H5P system after the last javascript is loaded.
                    H5P.init(_this);
                }
                createScriptElement();
            }
        }

        if (js.length === 0)
        {
            // If there are no javascript file to load, init the H5P system immediately, else init the H5P system after the last javascript is loaded.
            H5P.init(this);
        }
        else
        {
            createScriptElement();
        }
    }
}

// After initialization, put the content items that are previously finished in "solution mode".
H5P.externalDispatcher.on('initialized', function (_event)
{
    for (var i = 0; i < window.H5PInstances.length; i++)
    {
        const instance = window.H5PInstances[i];
        if (window.H5PAutoShowCompleted[instance.contentId] === true)
        {
            if (window.H5PCompleted[instance.contentId])
            {
                if (instance.showSolutions)
                {
                    instance.showSolutions();
                }
            }
        }
    }
});

H5P.externalDispatcher.on('xAPI', function (event)
{
    if (!window.H5PContentIds)
    {
        window.H5PContentIds = [];
    }

    if (!window.H5PInstances)
    {
        window.H5PInstances = [];
    }

    // Keep track of instance objects of the content items on this page. The instance object is only passed during xAPI events, so we need this opportunity to get a hold of them.
    const contentId = event.data.statement.object.definition.extensions['http://h5p.org/x-api/h5p-local-content-id'];
    const index = window.H5PContentIds.indexOf(contentId);
    if (index === -1)
    {
        window.H5PContentIds.push(contentId);
        window.H5PInstances.push(this);
    }
    else
    {
        // instance might have been changed, replace it
        window.H5PInstances[index] = this;
    }

    const element = $('h5p-content[content-id="' + contentId + '"]')[0];

    // If this content item has been completed, store the state of the item in a form field so we can process it in the postback of the page.
    if (this.getCurrentState)
    {
        // Path 1: Process content types that support state
        const state = this.getCurrentState();
        const stateJson = JSON.stringify(state);
        const result = event.data.statement.result;

        if (result)
        {
            if (element.onfinished)
            {
                element.onfinished(stateJson, result.score.scaled);
            }
        }
    }
    else
    {
        // Path 2: Process content types that do not support state
        if (event.data.statement.verb.id === "http://adlnet.gov/expapi/verbs/completed")
        {
            if (element.onfinished)
            {
                element.onfinished(null, null);
            }
        }
    }
});

H5P.preventInit = true;

window.H5PIntegration = {
    'baseUrl': 'http://www.mysite.com',     // No trailing slash
    'url': null,   // Relative to web root
    'postUserStatistics': false,             // Only if user is logged in
    'ajaxPath': null, // Only used by older Content Types
    'ajax': {
        // Where to post user results
        'setFinished': null,
        // Words beginning with : are placeholders
        'contentUserData': null,
    },
    'saveFreq': true, // How often current content state should be saved. false to disable.
    'user': { // Only if logged in !
        'name': 'User Name',
        'mail': 'user@mysite.com'
    },
    'siteUrl': 'http://www.mysite.com', // Only if NOT logged in!
    'l10n': { // Text string translations
        'H5P': {
            'fullscreen': 'Fullscreen',
            'disableFullscreen': 'Disable fullscreen',
            'download': 'Download',
            'copyrights': 'Rights of use',
            'embed': 'Embed',
            'size': 'Size',
            'showAdvanced': 'Show advanced',
            'hideAdvanced': 'Hide advanced',
            'advancedHelp': 'Include this script on your website if you want dynamic sizing of the embedded content:',
            'copyrightInformation': 'Rights of use',
            'close': 'Close',
            'title': 'Title',
            'author': 'Author',
            'year': 'Year',
            'source': 'Source',
            'license': 'License',
            'thumbnail': 'Thumbnail',
            'noCopyrights': 'No copyright information available for this content.',
            'downloadDescription': 'Download this content as a H5P file.',
            'copyrightsDescription': 'View copyright information for this content.',
            'embedDescription': 'View the embed code for this content.',
            'h5pDescription': 'Visit H5P.org to check out more cool content.',
            'contentChanged': 'This content has changed since you last used it.',
            'startingOver': 'You\'ll be starting over.',
            'by': 'by',
            'reuse': 'Reuse',
            'showMore': 'Show more',
            'showLess': 'Show less',
            'subLevel': 'Sublevel'
        }
    },
    'contents': []
};

window.customElements.define('h5p-content', H5PContent);
