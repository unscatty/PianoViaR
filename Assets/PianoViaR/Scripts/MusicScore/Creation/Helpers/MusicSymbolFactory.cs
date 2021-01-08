using System;
using UnityEngine;

namespace PianoViaR.Score.Helpers
{
    public enum SymbolType
    {
        NOTE_HEAD,
        NOTE_HEAD_HOLE,
        NOTE_FLAG_EIGHTH,
        NOTE_FLAG_SIXTEENTH,
        NOTE_FLAG_THIRTY_SECOND,
        NOTE_STEM,
        NOTE_BEAM,
        NOTE_DOT,
        NOTE_BAR,
        ACCIDENTAL_FLAT,
        ACCIDENTAL_NATURAL,
        ACCIDENTAL_SHARP,
        CLEF_BASS,
        CLEF_TREBLE,
        REST_WHOLE,
        REST_HALF,
        REST_QUARTER,
        REST_EIGHTH,
        REST_SIXTEENTH,
        REST_THIRTY_SECOND,
        STAFF_BAR,
        NOTE_NAME_TEXT,
        SIGNATURE_TEXT,
        MEASURES_TEXT,
        CHORD
    }

    public class MusicSymbolFactory : MonoBehaviour
    {
        [Header("Note components")]
        public GameObject noteHead;
        public GameObject noteHeadHole;
        public GameObject noteFlagEighth;
        public GameObject noteFlagSixteenth;
        public GameObject noteFlagThirtySecond;
        public GameObject noteStem;

        [Header("Miscellaneous")]
        public GameObject chord;
        public GameObject noteBeam;
        public GameObject noteDot;
        public GameObject noteBar;
        public GameObject staffBar;
        public GameObject noteNameText;
        public GameObject signatureText;
        public GameObject measuresText;

        [Header("Accidentals")]
        public GameObject accidentalFlat;
        public GameObject accidentalNatural;
        public GameObject accidentalSharp;

        [Header("Clefs")]
        public GameObject clefBass;
        public GameObject clefTreble;

        [Header("Rests")]
        public GameObject restWhole;
        public GameObject restHalf;
        public GameObject restQuarter;
        public GameObject restEighth;
        public GameObject restSixteenth;
        public GameObject restThirtySecond;

        public GameObject CreateSymbol(SymbolType symbolType)
        {
            return Instantiate(ChooseType(symbolType));
        }

        public GameObject CreateSymbol(SymbolType symbolType, Transform parent)
        {
            return Instantiate(ChooseType(symbolType), parent);
        }

        public GameObject CreateSymbol(SymbolType symbolType, Transform parent, bool worldPositionStays)
        {
            return Instantiate(ChooseType(symbolType), parent, worldPositionStays);
        }

        public GameObject CreateSymbol(SymbolType symbolType, Vector3 position, Quaternion rotation)
        {
            return Instantiate(ChooseType(symbolType), position, rotation);
        }

        public GameObject CreateSymbol(SymbolType symbolType, Vector3 position)
        {
            var choose = ChooseType(symbolType);
            return Instantiate(choose, position, choose.transform.rotation);
            // return CreateSymbol(symbolType, position, ChooseType(symbolType).transform.rotation);
        }

        public GameObject CreateSymbol(SymbolType symbolType, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Instantiate(ChooseType(symbolType), position, rotation, parent);
        }

        public GameObject CreateSymbol(SymbolType symbolType, Vector3 position, Transform parent)
        {
            return CreateSymbol(symbolType, position, parent.transform.rotation, parent);
        }

        public GameObject CreateSymbol(SymbolType symbolType, Vector2 xyPosition, Transform parent)
        {
            return CreateSymbol(symbolType, new Vector3(xyPosition.x, xyPosition.y, parent.transform.position.z), parent);
        }

        private GameObject ChooseType(SymbolType symbolType)
        {
            switch (symbolType)
            {
                case SymbolType.NOTE_HEAD:
                    return noteHead;

                case SymbolType.NOTE_HEAD_HOLE:
                    return noteHeadHole;

                case SymbolType.NOTE_FLAG_EIGHTH:
                    return noteFlagEighth;

                case SymbolType.NOTE_FLAG_SIXTEENTH:
                    return noteFlagSixteenth;

                case SymbolType.NOTE_FLAG_THIRTY_SECOND:
                    return noteFlagThirtySecond;

                case SymbolType.NOTE_STEM:
                    return noteStem;

                case SymbolType.NOTE_BEAM:
                    return noteBeam;

                case SymbolType.NOTE_DOT:
                    return noteDot;

                case SymbolType.NOTE_BAR:
                    return noteBar;

                case SymbolType.ACCIDENTAL_FLAT:
                    return accidentalFlat;

                case SymbolType.ACCIDENTAL_NATURAL:
                    return accidentalNatural;

                case SymbolType.ACCIDENTAL_SHARP:
                    return accidentalSharp;

                case SymbolType.CLEF_BASS:
                    return clefBass;

                case SymbolType.CLEF_TREBLE:
                    return clefTreble;

                case SymbolType.REST_WHOLE:
                    return restWhole;

                case SymbolType.REST_HALF:
                    return restHalf;

                case SymbolType.REST_QUARTER:
                    return restQuarter;

                case SymbolType.REST_EIGHTH:
                    return restEighth;

                case SymbolType.REST_SIXTEENTH:
                    return restSixteenth;

                case SymbolType.REST_THIRTY_SECOND:
                    return restThirtySecond;

                case SymbolType.STAFF_BAR:
                    return staffBar;

                case SymbolType.NOTE_NAME_TEXT:
                    return noteNameText;

                case SymbolType.SIGNATURE_TEXT:
                    return signatureText;

                case SymbolType.MEASURES_TEXT:
                    return measuresText;

                case SymbolType.CHORD:
                    return chord;

                default:
                    throw new ArgumentException($"Unknown type: {symbolType}");
            }
        }
    }
}