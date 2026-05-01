import { ExtensionContainer } from 'tessa/extensions';
ExtensionContainer.instance.registerBundle({
  name: 'Tessa.Extensions.Default.js',
  buildTime: process.env.BUILD_TIME!
});