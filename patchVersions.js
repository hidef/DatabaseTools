var fs = require('fs');

var globalJson = fs.readFileSync('global.json');
var globalConfig = JSON.parse(globalJson);

globalConfig.projects.map(function(project) {
    console.log('Patching:', project);
    var projectJson = fs.readFileSync(project + '/project.json');
    var projectConfig = JSON.parse(projectJson);
    projectConfig.version = process.env.APPVEYOR_BUILD_VERSION;
    fs.writeFileSync(project + '/project.json', JSON.stringify(projectConfig, null, 4));
});