var path = require('path');
var webpack = require('webpack');
var CleanWebpackPlugin = require('clean-webpack-plugin');
// var fs = require('fs');

// const examplesPath = './examples';
// const entry = fs.readdirSync(examplesPath).reduce((s, v) => {
//   s[v] = examplesPath + '/' + v;
//   return s;
// }, {});
const outputPath = path.join(__dirname, '/wwwroot/extensions');

module.exports = {
  context: path.join(__dirname),
  mode: 'production',
  devtool: 'source-map',
  entry: {
    default: './src/default/index.ts'
  },
  output: {
    path: outputPath,
    filename: '[name].[chunkhash].js'
  },
  externals: [
    {
      react: 'React',
      'react-dom': 'ReactDom',
      classnames: 'classnames',
      mobx: 'mobx',
      'mobx-utils': 'mobxUtils',
      'mobx-react': 'mobxReact',
      moment: 'Moment',
      'styled-components': 'styledComponents',
      'react-dropzone': 'ReactDropZone',
      'd3': 'd3',
      'history': 'history',
      'clipboard': 'Clipboard'
    },
    function(context, request, callback) {
      if (/^common|^ui|^components|^tessa/.test(request)) {
        return callback(null, 'var tessa.' + request.match(/\w+/g).join('.'));
      }
      callback();
    }
  ],
  module: {
    rules: [
      {
        test: /.jsx?$/,
        loader: 'babel-loader',
        exclude: /node_modules/
      },
      {
        test: /\.tsx?$/,
        loaders: ['babel-loader', 'ts-loader'],
        exclude: /node_modules/
      },
      {
        test: /\.css$/,
        use: [
          // мы хотим чтобы css-modules шли раньше styled-components
          { loader: 'style-loader', options: { insert: '#tessa-css-modules-root' } },
          { loader: 'css-loader' }
        ]
      },
    ]
  },
  plugins: [
    new CleanWebpackPlugin.CleanWebpackPlugin({
      verbose: true,
      cleanOnceBeforeBuildPatterns: [outputPath]
    }),
    new webpack.DefinePlugin({
      'process.env': {
        NODE_ENV: JSON.stringify('production'),
        BUILD_TIME: JSON.stringify(Date.now())
      }
    }),
  ],
  resolve: {
    extensions: ['.ts', '.tsx', '.js', '.jsx', '.css', '.scss'],
    modules: [path.resolve(__dirname), 'node_modules']
  }
};
