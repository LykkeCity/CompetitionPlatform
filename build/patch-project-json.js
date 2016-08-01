var jsonfile = require('jsonfile');

var file = '../src/competitionplatform/project.json';

jsonfile.readFile(file, function (err, project) {
    project.version = process.env.APPVEYOR_BUILD_VERSION;
    jsonfile.writeFile(file, project, {spaces: 2}, function(err) {
        console.error(err);
    });
})