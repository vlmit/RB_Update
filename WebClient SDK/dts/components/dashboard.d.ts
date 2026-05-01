import * as React from 'react';
import { Location } from 'history';
declare class Dashboard extends React.Component<DashboardProps> {
    componentDidMount(): void;
    componentWillUnmount(): void;
    onClick: (event: any) => void;
    onTouchStart: () => void;
    render(): JSX.Element;
}
export interface DashboardProps {
    location: Location<any>;
}
export default Dashboard;
