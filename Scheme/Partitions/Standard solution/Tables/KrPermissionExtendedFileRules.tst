<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="d1b372f3-7565-4309-9037-5e5a0969d94e" ID="7ca15c10-9fd1-46e9-8769-b0acc0efe118" Name="KrPermissionExtendedFileRules" Group="Kr" InstanceType="Cards" ContentType="Collections">
	<Description>Секция с расширенными настройками доступа к файлам</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7ca15c10-9fd1-00e9-2000-00acc0efe118" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7ca15c10-9fd1-01e9-4000-00acc0efe118" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7ca15c10-9fd1-00e9-3100-00acc0efe118" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="3dc7aaf0-eea0-4304-a229-ca72a011f637" Name="Extensions" Type="String(Max) Null" />
	<SchemeComplexColumn ID="d869fbf1-4153-496d-964e-2eeb701af579" Name="AccessSetting" Type="Reference(Typified) Not Null" ReferencedTable="95a74318-2e98-46bd-bced-1890dd1cd017">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d869fbf1-4153-006d-4000-0eeb701af579" Name="AccessSettingID" Type="Int32 Not Null" ReferencedColumn="7e9d555d-de8f-4b5c-9ad2-bc80ed062c12" />
		<SchemeReferencingColumn ID="72702106-5db1-4c24-aa8d-9491a0587c5f" Name="AccessSettingName" Type="String(Max) Not Null" ReferencedColumn="433c1052-0a35-47e3-b24a-44d08200cffb" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="2efa64be-0f56-4edf-ad1f-fa63f1d8abab" Name="CheckOwnFiles" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="6093b125-8aaa-4064-b410-4ae276f88c0f" Name="df_KrPermissionExtendedFileRules_CheckOwnFiles" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="18a0235d-dc17-4b79-8f3a-2640761e2762" Name="Order" Type="Int32 Not Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="7ca15c10-9fd1-00e9-5000-00acc0efe118" Name="pk_KrPermissionExtendedFileRules">
		<SchemeIndexedColumn Column="7ca15c10-9fd1-00e9-3100-00acc0efe118" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="7ca15c10-9fd1-00e9-7000-00acc0efe118" Name="idx_KrPermissionExtendedFileRules_ID" IsClustered="true">
		<SchemeIndexedColumn Column="7ca15c10-9fd1-01e9-4000-00acc0efe118" />
	</SchemeIndex>
</SchemeTable>