import React from 'react';
import { Location } from 'history';
declare type TessaAppProps = {
    location: Location<any>;
};
declare class TessaApp extends React.Component<TessaAppProps> {
    render(): JSX.Element;
}
export default TessaApp;
