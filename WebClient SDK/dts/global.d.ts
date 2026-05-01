declare type guid = string;

declare var process: {
  env: {
    [key: string]: string | undefined;
  };
};

// declare namespace Guid {
//   const empty: guid;
//   function equals(a: guid | null | undefined, b: guid | null | undefined): boolean;
//   function newGuid(): guid;
//   function validate(id: guid | null | undefined): boolean;
//   function isEmpty(id: guid | null | undefined): boolean;
// }
