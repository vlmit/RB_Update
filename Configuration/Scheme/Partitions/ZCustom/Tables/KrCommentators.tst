<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="42c4f5aa-d0e8-4d26-abb8-a898e736fe35" Partition="d1b372f3-7565-4309-9037-5e5a0969d94e">
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="11ae81e4-1764-4d19-8c07-71b2846c2399" Name="Order" Type="Int32 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="80c1e917-5c72-4a31-aadf-30f40c8e2a8b" Name="df_KrCommentators_Order" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="2b0129a9-5373-433c-9368-0ba9382d950f" Name="IsResponsible" Type="Boolean Null">
		<Description>Статус основного главного исполнителя</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="40beffae-ed57-4af2-bad1-196982d7e254" Name="df_KrCommentators_IsResponsible" Value="false" />
	</SchemePhysicalColumn>
</SchemeTable>