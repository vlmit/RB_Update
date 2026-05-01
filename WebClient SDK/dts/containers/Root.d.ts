import React from 'react';
import { History } from 'history';
declare type RootProps = {
    history: History;
};
export default class Root extends React.Component<RootProps> {
    render(): JSX.Element;
}
export {};
